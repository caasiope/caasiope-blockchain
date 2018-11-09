using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetPeersCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var peers = ConnectionService.GetConnectedPeers().ToList();
            Console.WriteLine($"Number of connected peers : {peers.Count}");
            foreach (var peer in peers)
            {
                var elapsed = Math.Round((DateTime.Now - peer.LastPong).TotalMilliseconds);
                Console.WriteLine($"Peer ID : {peer.ID} IP : {peer.IP}, Height : {peer.Height}, Ping : {peer.Ping}, Last Pong : {elapsed}");
            }
        }
    }
}