using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SquadCSharpDiscord
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Boolean userJoining = false;
            _BackEnd logCrawler = new _BackEnd();
            Regex rg;
            string fileName = "/Steam/Squad/Server-1/SquadGame/Saved/SquadGame.log";
            Console.WriteLine("The Application is Live, the Crawler is on the move.");
            Boolean done = true;

            string connetionString = null;
            string _sql = "";
            connetionString = "server=localhost;user=squadC;Password=squad;port=3306;Database=testdb";
            MySqlConnection conn = new MySqlConnection(connetionString);
            //try
            //{
            //    conn.Open();
            //    Console.WriteLine("Connection Open ! ");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Can not open connection ! ");
            //}

            string lineReturn = "";
            using (StreamReader reader = new StreamReader(new FileStream(fileName,
                     FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                long lastMaxOffset = reader.BaseStream.Length;

                while (done)
                {
                    System.Threading.Thread.Sleep(100);
                    string line = "";
                    string[] subStrings;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) | Regex.IsMatch(line, "SendOutboundMessage")) { continue; }
                        else
                        {
                            foreach (var tester in logCrawler._AllPatterns)
                            {
                                rg = new Regex(tester.Value);
                                Match match = rg.Match(line);
                                if (match.Success)
                                {
                                    if (userJoining & Regex.IsMatch(match.Value, "LogEasyAntiCheatServer"))
                                    {
                                        subStrings = Regex.Split(line, tester.Value);
                                        logCrawler.matchList("UserJoining", match.Value, subStrings, conn, userJoining);
                                        lineReturn = match.Value;
                                        userJoining = false;
                                        break;
                                    }
                                    else if (Regex.IsMatch(match.Value, "NewPlayer: BP_PlayerController_C"))
                                    {
                                        subStrings = Regex.Split(line, tester.Value);
                                        logCrawler.matchList("playerConnected", match.Value, subStrings, conn);
                                        lineReturn = match.Value;
                                        userJoining = true;
                                        break;
                                    }
                                    else if (!lineReturn.Equals(match.Value))
                                    {
                                        subStrings = Regex.Split(line, tester.Value);
                                        logCrawler.matchList(tester.Key, match.Value, subStrings, conn);

                                        lineReturn = match.Value;
                                    }
                                }
                            }
                        }

                    }
                    //        //Console.WriteLine(lineReturn);
                    //        //update the last max offset
                    //        lastMaxOffset = reader.BaseStream.Position;
                    //    }
                    //}
                }
            }
        }
    }
}
