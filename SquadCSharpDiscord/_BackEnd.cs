using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SquadCSharpDiscord
{
    class _BackEnd
    {

        public Dictionary<string, string> _AllPatterns;

        //C_ID = Key, UserName = Value
        public Dictionary<string, string> C_IDWithUser;

        //SteamID = Key, C_ID = Value
        private Dictionary<string, string> SteamWithC_ID;

        //UserName = Key, TeamID = Value
        public Dictionary<string, string> UserWithTeam;
        public List<string> adminInCameraList;
        public Dictionary<string, string> adminInCameraDic;
        private string C_ID;
        private string steamID;

        public _BackEnd()
        {
            C_ID = "";
            adminInCameraList = new List<string>();
            adminInCameraDic = new Dictionary<string, string>();
            UserWithTeam = new Dictionary<string, string>();

            C_IDWithUser = new Dictionary<string, string>();
            SteamWithC_ID = new Dictionary<string, string>();

            _AllPatterns = new Dictionary<string, string>();
            regexSetup();
        }

        private void regexSetup()
        {
            _AllPatterns.Add("playerConnected", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: PostLogin: NewPlayer: BP_PlayerController_C (.+).BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("steamID", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogEasyAntiCheatServer: \\[[0-9:]+]\\[[A-z]+]\\[EAC Server] \\[Info]\\[RegisterClient] Client: ([A-z0-9]+) PlayerGUID: ([0-9]{17}) PlayerIP: [0-9]{17} OwnerGUID: [0-9]{17} PlayerName: (.+)");
            _AllPatterns.Add("playerName", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogNet: Join succeeded: (.+)");
            _AllPatterns.Add("chatMessage", "\\[(ChatAll|ChatTeam|ChatSquad|ChatAdmin)] \\[SteamID:([0-9]{17})] (.+?) : (.*)");
            _AllPatterns.Add("removeUser", "\\[([0-9.:-]+)][[ 0-9]+]LogNet: UChannel::Close: [A-z0-9_ ,.=:]+ RemoteAddr: ([0-9]+):[A-z0-9_ ,.=:]+ BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("adminBroadcast", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: ADMIN COMMAND: Message broadcasted <(.+)> from (.+)");
            _AllPatterns.Add("newGame", "\\[([0-9.:-]+)][[ 0-9]*]LogWorld: Bringing World \\/([A-z]+)\\/Maps\\/([A-z]+)\\/(?:Gameplay_Layers\\/)?([A-z0-9_]+)");
            _AllPatterns.Add("playerDamaged", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: Player:(.+) ActualDamage=([0-9.]+) from (.+) caused by ([A-z_0-9]+)_C");
            _AllPatterns.Add("playerDied", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQSoldier::)?Die\\(\\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from BP_PlayerController_([A-z_0-9]+) caused by ([A-z_0-9]+)");
            _AllPatterns.Add("playerPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQPlayerController::)?OnPossess\\(\\): PC=(.+) Pawn=([A-z0-9_]+)_C");
            _AllPatterns.Add("playerUnPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQPlayerController::)?OnUnPossess\\(\\): PC=(.+)");
            _AllPatterns.Add("playerRevived", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: (.+) has revived (.+)\\.");
            _AllPatterns.Add("playerWounded", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQSoldier::)?Wound\\(\\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from ([A-z_0-9]+) caused by ([A-z_0-9]+)_C");
            //_AllPatterns.Add("serverTick", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: USQGameState: Server Tick Rate: ([0-9.]+)");
            _AllPatterns.Add("roundWinner", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer]ASQGameMode::DetermineMatchWinner\\(\\): (.+) won on (.+)");
            _AllPatterns.Add("playerList", "/ID: ([0-9]+) \\| SteamID: ([0-9]{17}) \\| Name: (.+) \\| Team ID: ([0-9]+) \\| Squad ID: ([0-9]+|N\\/A)");
            _AllPatterns.Add("currentMap", "/^Current map is (.+), Next map is (.*)/");
        }


        public void matchList(string stringType, string line, string[] substring, MySqlConnection conn, Boolean newUser = false)
        {
            String _SQL;
            switch (stringType)
            {
                case "playerDied":
                    if (substring[5].Equals("nullptr"))
                        break;
                    string victimResult, attackerResult, cIDResult;
                    if (UserWithTeam.TryGetValue(substring[2], out victimResult))
                    {
                        C_IDWithUser.TryGetValue(substring[4], out cIDResult);
                        if (String.IsNullOrEmpty(cIDResult)) { attackerResult = ""; } else { UserWithTeam.TryGetValue(cIDResult, out attackerResult); }


                        if (String.IsNullOrEmpty(attackerResult))
                        {
                            Console.WriteLine("There was a Potential TeamKill");
                            Console.WriteLine("The Victim Team was: " + victimResult);
                            Console.WriteLine(line);
                            break;
                        }
                        if (String.IsNullOrEmpty(victimResult)) { }

                        if (victimResult.Equals(attackerResult))
                        {
                            Console.WriteLine("There was a TeamKill");
                            Console.WriteLine(line);
                        }
                    }
                    break;
                case "playerName":
                    C_IDWithUser[C_ID] = substring[2];
                    //if (steamID.Equals(""))
                    //    steamID = "00000000000000000";
                    //_SQL = "INSERT IGNORE INTO userNameList (steamID, userName) VALUES (@steamID, @userName);";
                    //cmd = new MySqlCommand(_SQL, conn);
                    //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                    //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                    //cmd.ExecuteNonQuery();

                    //_SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                    //         ON DUPLICATE KEY UPDATE
                    //         userName = VALUES(userName),
                    //         connected = VALUES(connected)";
                    //cmd = new MySqlCommand(_SQL, conn);
                    //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                    //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                    //cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = 1;
                    //cmd.ExecuteNonQuery();

                    steamID = "";
                    C_ID = "";
                    break;
                case "playerConnected":
                    C_ID = substring[3];
                    if (substring[3].Equals("C_2146556060"))
                        Console.WriteLine("He was added?");
                    //Console.WriteLine(C_ID);
                    //Adding the User C_ID to the Dictionary First, since we don't know the UserName yet. 
                    C_IDWithUser.Add(substring[3], "not defined");

                    break;
                case "UserJoining":
                    if (newUser)
                    {
                        if (String.IsNullOrEmpty(C_ID))
                        {
                            break;
                        }
                        steamID = substring[3];
                        SteamWithC_ID.Add(substring[3], C_ID);
                        //try
                        //{
                        //    _SQL = "INSERT IGNORE INTO steamuser (steamID) VALUES (@steamID);";
                        //    cmd = new MySqlCommand(_SQL, conn);
                        //    cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                        //    cmd.ExecuteNonQuery();



                        //    _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                        //    cmd = new MySqlCommand(_SQL, conn);
                        //    cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                        //    cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Connected";
                        //    cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4] + " joined the server";
                        //    cmd.ExecuteNonQuery();
                        //}
                        //catch (Exception e)
                        //{
                        //    Console.WriteLine(e);
                        //}
                    }
                    break;
                case "removeUser":
                    if (SteamWithC_ID.ContainsKey(substring[2]))
                    {
                        if (adminInCameraDic.ContainsKey(C_IDWithUser[SteamWithC_ID[substring[2]]]))
                            adminInCameraDic[C_IDWithUser[SteamWithC_ID[substring[2]]]] = "Inactive";
                        //try
                        //{
                        //    _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                        //    //Console.WriteLine(_SQL);
                        //    cmd = new MySqlCommand(_SQL, conn);
                        //    cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        //    cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Disconnected";
                        //    cmd.Parameters.Add("@message", MySqlDbType.Text).Value = C_IDWithUser[SteamWithC_ID[substring[2]]] + " left the server";
                        //    cmd.ExecuteNonQuery();

                        //    _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                        //             ON DUPLICATE KEY UPDATE
                        //             userName = VALUES(userName),
                        //             connected = VALUES(connected)";
                        //    cmd = new MySqlCommand(_SQL, conn);
                        //    cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        //    cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = C_IDWithUser[SteamWithC_ID[substring[2]]];
                        //    cmd.Parameters.Add("@connected", MySqlDbType.UInt32).Value = 0;
                        //    cmd.ExecuteNonQuery();
                        //}
                        //catch (Exception e)
                        //{

                        //    Console.WriteLine(e);
                        //}
                        C_IDWithUser.Remove(SteamWithC_ID[substring[2]]);
                        SteamWithC_ID.Remove(substring[2]);
                    }
                    break;
                case "playerPosses":
                    string bp_soldier = "bp_soldier";

                    if (substring[3].ToLower().StartsWith(bp_soldier))
                    {
                        string[] teamID = substring[3].Split("_");
                        if (UserWithTeam.ContainsKey(substring[2]))
                        {
                            UserWithTeam[substring[2]] = teamID[2];
                        }
                        else
                        {
                            UserWithTeam.Add(substring[2], teamID[2]);
                        }

                    }
                    else if (substring[3].ToLower().StartsWith("cameraman"))
                    {
                        adminInCameraList.Add(line);
                        if (adminInCameraDic.ContainsKey(substring[2]))
                        {
                            adminInCameraDic[substring[2]] = "Active";
                        }
                        else
                        {
                            adminInCameraDic.Add(substring[2], "Active");
                        }
                        //_SQL = "INSERT INTO adminlog (userName, logMessage) VALUES (@userName, @logMessage)";
                        //cmd = new MySqlCommand(_SQL, conn);
                        //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                        //cmd.Parameters.Add("@logMessage", MySqlDbType.Text).Value = line;
                        //cmd.ExecuteNonQuery();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
