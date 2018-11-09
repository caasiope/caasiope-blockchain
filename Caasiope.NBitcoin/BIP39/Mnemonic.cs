#pragma warning disable CS0618 // Type or member is obsolete
using System;
using System.Linq;
using System.Text;

#if !WINDOWS_UWP && !USEBC
#endif

namespace Caasiope.NBitcoin.BIP39
{
	/// <summary>
	/// A .NET implementation of the Bitcoin Improvement Proposal - 39 (BIP39)
	/// BIP39 specification used as reference located here: https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki
	/// Made by thashiznets@yahoo.com.au
	/// v1.0.1.1
	/// I ♥ Bitcoin :)
	/// Bitcoin:1ETQjMkR1NNh4jwLuN5LxY7bMsHC9PUPSV
	/// </summary>
	public class Mnemonic
	{
		public Mnemonic(string mnemonic, Wordlist wordlist = null)
		{
			if(mnemonic == null)
				throw new ArgumentNullException(nameof(mnemonic));
			this.mnemonic = mnemonic.Trim();

			if(wordlist == null)
				wordlist = Wordlist.AutoDetect(mnemonic) ?? Wordlist.English;

			var words = mnemonic.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
			//if the sentence is not at least 12 characters or cleanly divisible by 3, it is bad!
			if(!WORD_COUNT.Contains(words.Length))
			{
				throw new FormatException($"Word count should be {string.Join(",", WORD_COUNT)}");
			}
			Words = words;
			WordList = wordlist;
			Indices = wordlist.ToIndices(words);
		}

        /// <summary>
        /// Generate a mnemonic
        /// </summary>
        /// <param name="wordList"></param>
        /// <param name="entropy"></param>
        public Mnemonic(Wordlist wordList, byte[] entropy)
		{
			WordList = wordList ?? Wordlist.English;

            if (!ENTROPY_LENGTHS.Contains(entropy.Length * 8))
				throw new ArgumentException($"The length for entropy should be : {ENTROPY_LENGTHS}", nameof(entropy));

			// var checksum = Hashes.SHA256(entropy);
			var entcsResult = new BitWriter();

			entcsResult.Write(entropy);
			// entcsResult.Write(checksum, CHECKSUM_LENGTH) // no need hash for now
			Indices = Wordlist.ToIntegers(entcsResult.ToBitArray());
            Words = WordList.GetWords(Indices);
			mnemonic = WordList.GetSentence(Indices);
        }

	    private readonly int[] WORD_COUNT = {24, 28};
        // private const int CHECKSUM_LENGTH = 8;
	    private readonly int[] ENTROPY_LENGTHS = {256, 304};

	    private readonly string mnemonic;
	    private Wordlist WordList { get; }
	    private int[] Indices { get; }

	    private bool? isValidChecksum;

	    public bool IsValidChecksum
		{
			get
			{
				if(isValidChecksum == null)
				{
					var writer = new BitWriter();
					var bits = Wordlist.ToBits(Indices);

				    var ent = ENTROPY_LENGTHS[Array.IndexOf(WORD_COUNT, Indices.Length)];

                    writer.Write(bits, ent);
					var entropy = writer.ToBytes();
					// var checksum = Hashes.SHA256(entropy);

					// writer.Write(checksum, CHECKSUM_LENGTH);
					var expectedIndices = Wordlist.ToIntegers(writer.ToBitArray());
					isValidChecksum = expectedIndices.SequenceEqual(Indices);
				}
				return isValidChecksum.Value;
			}
		}

	    public string[] Words { get; }

	    private static readonly Encoding NoBomutf8 = new UTF8Encoding(false);

	    public byte[] DeriveData()
		{
		    var writer = new BitWriter();
            var bits = Wordlist.ToBits(Indices);

		    var ent = ENTROPY_LENGTHS[Array.IndexOf(WORD_COUNT, Indices.Length)];

            writer.Write(bits, ent);
		    return writer.ToBytes();
		}

	    internal static byte[] Normalize(string str)
		{
			return NoBomutf8.GetBytes(NormalizeString(str));
		}

	    internal static string NormalizeString(string word)
		{
#if !NOSTRNORMALIZE
			if(!SupportOsNormalization())
			{
				return KDTable.NormalizeKD(word);
			}
			else
			{
				return word.Normalize(NormalizationForm.FormKD);
			}
#else
			return KDTable.NormalizeKD(word);
#endif
		}

#if !NOSTRNORMALIZE
	    private static bool? _supportOsNormalization;
	    internal static bool SupportOsNormalization()
		{
			if(_supportOsNormalization == null)
			{
				const string notNormalized = "あおぞら";
				const string normalized = "あおぞら";
				if(notNormalized.Equals(normalized, StringComparison.Ordinal))
				{
					_supportOsNormalization = false;
				}
				else
				{
					try
					{
						_supportOsNormalization = notNormalized.Normalize(NormalizationForm.FormKD).Equals(normalized, StringComparison.Ordinal);
					}
					catch { _supportOsNormalization = false; }
				}
			}
			return _supportOsNormalization.Value;
		}
#endif


	    private static byte[] Concat(byte[] source1, byte[] source2)
		{
			//Most efficient way to merge two arrays this according to http://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
			var buffer = new byte[source1.Length + source2.Length];
			Buffer.BlockCopy(source1, 0, buffer, 0, source1.Length);
			Buffer.BlockCopy(source2, 0, buffer, source1.Length, source2.Length);

			return buffer;
		}

	    public override string ToString()
		{
			return mnemonic;
		}
	}
}
#pragma warning restore CS0618 // Type or member is obsolete