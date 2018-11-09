function ReplaceFromFile 
{
	 param($path, $oldStr, $newStr)
	 $result = Get-Content -Path $templateFile -Encoding UTF8 |
	 %{$_ -replace $oldStr, $newStr };
	 return $result;
}

function TryFindGit 
{
	# set git alias
	# if it's git64
	If (Test-Path "$env:ProgramFiles\Git\bin\git.exe")
	{
		New-Alias -Name git -Value "$env:ProgramFiles\Git\bin\git.exe"
		return "y";
	}
	ElseIf (Test-Path "${env:ProgramFiles(x86)}\Git\bin\git.exe")
	{
		New-Alias -Name git -Value "${env:ProgramFiles(x86)}\Git\bin\git.exe"
		return "y";
	}

	if(!(test-path alias:git))
	{
		echo "git alias not found";
		return "n";
	}
}

if (!(Get-Command git -errorAction SilentlyContinue))
{
	$gitFound = TryFindGit;
	if($gitFound -eq "n")
	{
		return;
	}
}

$repoDir = (get-item $args[0].TrimEnd("`"")).parent.FullName + "\.git"

# Get version info from Git. example 1.2.3-45-g6789abc
$gitVersion = git --git-dir=$repoDir describe --long --always;

if ([string]::IsNullOrEmpty($gitVersion))
{
	return;
}

# Parse Git version info into semantic pieces
#$gitVersion -match '(.*)-(\d+)-[g](\w+)$';
$gitSHA1 = $gitVersion; # $Matches[3];

echo "sha1 : " $gitSHA1;

$path = $args[0].TrimEnd("`"");
$templateFile = $path + "\Properties\AssemblyInfo.cs";

If($args[1] -eq "revert") 
{
	
	# Read template file, overwrite place holders with git version info
	$assemblyContent = ReplaceFromFile $templateFile $gitSHA1 %ASSEMBLYINFORMATIONALVERSION%;
	$assemblyContent > $templateFile;  
	return;
}

# Read template file, overwrite place holders with git version info
$newAssemblyContent = ReplaceFromFile $templateFile %ASSEMBLYINFORMATIONALVERSION% $gitSHA1;

If([string]::IsNullOrEmpty($newAssemblyContent)){
	return;
}

# Write AssemblyInfo.cs file only if there are changes
If (-not (Test-Path $templateFile) -or ((Compare-Object (Get-Content $templateFile) $newAssemblyContent))) {
    echo "Injecting Git Version Info to AssemblyInfo.cs"
    $newAssemblyContent > $templateFile;       
}