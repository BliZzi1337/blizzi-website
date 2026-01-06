using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline
{
    private static readonly HttpClient client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public bool Execute()
    {
        // =====================================================
        CPH.TryGetArg("repo", out string repo);
        CPH.TryGetArg("token", out string token);
        // =====================================================

        bool success = true;
        success &= UpdateGold(repo, token);
        success &= UpdateCoinflips(repo, token);
        success &= UpdateDuels(repo, token);
        success &= UpdateWatchtime(repo, token);
        success &= UpdateMarbles(repo, token);

        return success;
    }

    // =====================================================
    public bool UpdateGold(string repo, string token)
    {
        try
        {
            var goldUserInfo = CPH.GetTwitchUsersVar<long>("points", true);

            if (goldUserInfo == null || goldUserInfo.Count == 0)
            {
                CPH.LogInfo("No gold data found for any users.");
                return false;
            }

            var sortedUsers = goldUserInfo
                .OrderByDescending(u => u.Value)
                .Select(u => new
                {
                    UserName = u.UserName,
                    UserLogin = u.UserLogin,
                    Gold = u.Value.ToString()
                })
                .ToList();

            var jsonData = new
            {
                config = new
                {
                    displayName = "GOLD",
                    columns = new[]
                    {
                        new
                        {
                            key = "Gold",
                            label = "Gold",
                            format = "number"
                        }
                    },
                    defaultSortColumn = "Gold"
                },
                data = sortedUsers
            };

            string jsonContent = JsonConvert.SerializeObject(jsonData, Formatting.Indented);

            bool success = UploadToGitHub(repo, token, "public/api/leaderboards/gold.json", jsonContent, "Update gold leaderboard");

            if (success)
            {
                CPH.LogInfo($"Successfully updated gold leaderboard with {sortedUsers.Count} users.");
                return true;
            }
            else
            {
                CPH.LogError("Failed to upload gold leaderboard to GitHub.");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error updating gold leaderboard: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    public bool UpdateCoinflips(string repo, string token)
    {
        try
        {
            var userVarList = CPH.GetTwitchUsersVar<string>("tawmae_data_TWITCH POINTS", true);

            if (userVarList == null || userVarList.Count == 0)
            {
                CPH.LogInfo("No coinflip data found for any users.");
                return false;
            }

            var coinflipData = new List<object>();

            foreach (var userVar in userVarList)
            {
                try
                {
                    var jsonData = JObject.Parse(userVar.Value);
                    var gambling = jsonData["gambling"];

                    if (gambling != null)
                    {
                        int total = gambling["total"]?.ToObject<int>() ?? 0;
                        int wins = gambling["wins"]?.ToObject<int>() ?? 0;
                        int losses = gambling["losses"]?.ToObject<int>() ?? 0;

                        if (total > 0)
                        {
                            coinflipData.Add(new
                            {
                                UserName = userVar.UserName,
                                UserLogin = userVar.UserLogin,
                                Gesamt = total.ToString(),
                                Gewonnen = wins.ToString(),
                                Verloren = losses.ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    CPH.LogWarn($"Failed to parse coinflip data for {userVar.UserName}: {ex.Message}");
                }
            }

            var sortedData = coinflipData.OrderByDescending(d => int.Parse(((dynamic)d).Gesamt)).ToList();

            var jsonObject = new
            {
                config = new
                {
                    displayName = "COINFLIPS",
                    columns = new[]
                    {
                        new { key = "Gesamt", label = "Gesamt", format = "number" },
                        new { key = "Gewonnen", label = "Gewonnen", format = "number" },
                        new { key = "Verloren", label = "Verloren", format = "number" }
                    },
                    defaultSortColumn = "Gesamt"
                },
                data = sortedData
            };

            string jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            bool success = UploadToGitHub(repo, token, "public/api/leaderboards/coinflips.json", jsonContent, "Update coinflips leaderboard");

            if (success)
            {
                CPH.LogInfo($"Successfully updated coinflips leaderboard with {sortedData.Count} users.");
                return true;
            }
            else
            {
                CPH.LogError("Failed to upload coinflips leaderboard to GitHub.");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error updating coinflips leaderboard: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    public bool UpdateDuels(string repo, string token)
    {
        try
        {
            var userVarList = CPH.GetTwitchUsersVar<string>("tawmae_data_TWITCH POINTS", true);

            if (userVarList == null || userVarList.Count == 0)
            {
                CPH.LogInfo("No duel data found for any users.");
                return false;
            }

            var duelData = new List<object>();

            foreach (var userVar in userVarList)
            {
                try
                {
                    var jsonData = JObject.Parse(userVar.Value);
                    var duels = jsonData["duels"];

                    if (duels != null)
                    {
                        int total = duels["total"]?.ToObject<int>() ?? 0;
                        int won = duels["won"]?.ToObject<int>() ?? 0;
                        int lost = duels["lost"]?.ToObject<int>() ?? 0;

                        if (total > 0)
                        {
                            duelData.Add(new
                            {
                                UserName = userVar.UserName,
                                UserLogin = userVar.UserLogin,
                                Gesamt = total.ToString(),
                                Gewonnen = won.ToString(),
                                Verloren = lost.ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    CPH.LogWarn($"Failed to parse duel data for {userVar.UserName}: {ex.Message}");
                }
            }

            var sortedData = duelData.OrderByDescending(d => int.Parse(((dynamic)d).Gesamt)).ToList();

            var jsonObject = new
            {
                config = new
                {
                    displayName = "DUELLE",
                    columns = new[]
                    {
                        new { key = "Gesamt", label = "Gesamt", format = "number" },
                        new { key = "Gewonnen", label = "Gewonnen", format = "number" },
                        new { key = "Verloren", label = "Verloren", format = "number" }
                    },
                    defaultSortColumn = "Gesamt"
                },
                data = sortedData
            };

            string jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            bool success = UploadToGitHub(repo, token, "public/api/leaderboards/duelle.json", jsonContent, "Update duels leaderboard");

            if (success)
            {
                CPH.LogInfo($"Successfully updated duels leaderboard with {sortedData.Count} users.");
                return true;
            }
            else
            {
                CPH.LogError("Failed to upload duels leaderboard to GitHub.");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error updating duels leaderboard: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    public bool UpdateWatchtime(string repo, string token)
    {
        try
        {
            var watchtimeUserInfo = CPH.GetTwitchUsersVar<long>("watchtime", true);

            if (watchtimeUserInfo == null || watchtimeUserInfo.Count == 0)
            {
                CPH.LogInfo("No watchtime data found for any users.");
                return false;
            }

            var sortedUsers = watchtimeUserInfo
                .OrderByDescending(u => u.Value)
                .Select(u => new
                {
                    UserName = u.UserName,
                    UserLogin = u.UserLogin,
                    Watchtime = u.Value.ToString()
                })
                .ToList();

            var jsonData = new
            {
                config = new
                {
                    displayName = "WATCHTIME",
                    columns = new[]
                    {
                        new
                        {
                            key = "Watchtime",
                            label = "Watchtime",
                            format = "time"
                        }
                    },
                    defaultSortColumn = "Watchtime"
                },
                data = sortedUsers
            };

            string jsonContent = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            bool success = UploadToGitHub(repo, token, "public/api/leaderboards/watchtime.json", jsonContent, "Update watchtime leaderboard");

            if (success)
            {
                CPH.LogInfo($"Successfully updated watchtime leaderboard with {sortedUsers.Count} users.");
                return true;
            }
            else
            {
                CPH.LogError("Failed to upload watchtime leaderboard to GitHub.");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error updating watchtime leaderboard: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    public bool UpdateMarbles(string repo, string token)
    {
        try
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string csvPath = System.IO.Path.Combine(basePath, "Marbles", "Stats.csv");

            if (!System.IO.File.Exists(csvPath))
            {
                CPH.LogInfo($"Marbles CSV file not found at: {csvPath}");
                return false;
            }

            var lines = System.IO.File.ReadAllLines(csvPath);

            if (lines.Length <= 1)
            {
                CPH.LogInfo("No marble data found in CSV.");
                return false;
            }

            var marblesData = new List<object>();

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');

                if (parts.Length < 7)
                {
                    CPH.LogWarn($"Skipping invalid line {i}: {lines[i]}");
                    continue;
                }

                string displayName = parts[0].Trim();
                string wins = parts[2].Trim();
                string points = parts[3].Trim();
                string seasonPoints = parts[4].Trim();
                string eliminations = parts[5].Trim();
                string totalRaces = parts[6].Trim();

                marblesData.Add(new
                {
                    UserName = displayName,
                    UserLogin = displayName.ToLower(),
                    Gesamt = totalRaces,
                    Wins = wins,
                    Eliminations = eliminations,
                    Points = points,
                    SeasonPoints = seasonPoints
                });
            }

            var sortedData = marblesData.OrderByDescending(d => int.Parse(((dynamic)d).Gesamt)).ToList();

            var jsonObject = new
            {
                config = new
                {
                    displayName = "MARBLES",
                    columns = new[]
                    {
                        new { key = "Gesamt", label = "Gesamt", format = "number" },
                        new { key = "Wins", label = "Wins", format = "number" },
                        new { key = "Eliminations", label = "Eliminations", format = "number" },
                        new { key = "Points", label = "Points", format = "number" },
                        new { key = "SeasonPoints", label = "Season Points", format = "number" }
                    },
                    defaultSortColumn = "Gesamt"
                },
                data = sortedData
            };

            string jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            bool success = UploadToGitHub(repo, token, "public/api/leaderboards/marbles.json", jsonContent, "Update marbles leaderboard");

            if (success)
            {
                CPH.LogInfo($"Successfully updated marbles leaderboard with {sortedData.Count} users.");
                return true;
            }
            else
            {
                CPH.LogError("Failed to upload marbles leaderboard to GitHub.");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error updating marbles leaderboard: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    private bool UploadToGitHub(string repo, string token, string filePath, string content, string commitMessage)
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{repo}/contents/{filePath}";

            string sha = GetFileSha(apiUrl, token);

            string base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

            var requestBody = new
            {
                message = commitMessage,
                content = base64Content,
                sha = sha
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Put, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("User-Agent", "Streamer.bot-Leaderboard-Updater");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                CPH.LogInfo($"Successfully uploaded {filePath} to GitHub.");
                return true;
            }
            else
            {
                string errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                CPH.LogError($"GitHub API error ({response.StatusCode}): {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"Error uploading to GitHub: {ex.Message}");
            return false;
        }
    }

    // =====================================================
    private string GetFileSha(string apiUrl, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("User-Agent", "Streamer.bot-Leaderboard-Updater");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var json = JObject.Parse(responseContent);
                return json["sha"]?.ToString();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                CPH.LogInfo($"Could not get file SHA: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            CPH.LogInfo($"Error getting file SHA (file might not exist yet): {ex.Message}");
            return null;
        }
    }
}
