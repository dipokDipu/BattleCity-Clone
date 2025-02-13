using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
//using PlayFab.PfEditor.Json;
using UnityEngine;
using UnityEngine.Events;
using JsonObject = PlayFab.Json.JsonObject;
using TMPro;

public class PlayFabController : MonoBehaviour
{
    public static PlayFabController Instance;

    public int x;
    #region LoginData

    private string _userEmail = " ";
    private string _userPassword = " ";
    private string _confirmUserPassword = "";
    private string _userName = " ";
    public TextMeshProUGUI ErrorReport;
    private bool loginEvent = true;

    public GameObject SignInOption;
    //public GameObject LoginPanel;
    public GameObject StartingPanel;

    #endregion


    #region Player_Data_To_Be_Stored

    public int TotalHighScore;
    public int PlayerCompletedLevel;
    public int PlayerHighScore;
    public int PlayerId;

    #endregion

    #region LeaderBoard_UI_Data


    public GameObject LeaderBoardPanel;
    public GameObject LeaderBoardRowPrefab;
    public GameObject LeaderBoardContainer;
    public UnityEvent LeaderBoardHighScore;
    public UnityEvent LoadLeaderBoardScore;

    #endregion



    public void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }

        Initialize();
    }

    public void Initialize()
    {

            LoadLeaderBoardScore?.Invoke();

            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = "A66CC";
            }

            if (PlayerPrefs.HasKey("LoginName"))
            {

                SetUserEmail(PlayerPrefs.GetString("LoginEmail"));
                SetUserName(PlayerPrefs.GetString("LoginName"));
                SetUserPassWord(PlayerPrefs.GetString("LoginPassword"));

                //var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };

                var request = new LoginWithPlayFabRequest {Username = _userName, Password = _userPassword};

                //PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);

                PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
            }
            



    }



    #region Player_Login

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        /*if (LoginPanel == null)
        {
            return;
        }*/

        SignInOption.SetActive(false);
        //LoginPanel.SetActive(false);
        StartingPanel.SetActive(true);
        PlayerPrefs.SetString("LoginEmail", _userEmail);
        PlayerPrefs.SetString("LoginPassword", _userPassword);
        PlayerPrefs.SetString("LoginName", _userName);
        PanelData.Instance.ShowLoginPanel = false;
        GetStats();
        GetHighScore();
    }

    private async void OnLoginFailure(PlayFabError error)
    {
        
        Debug.LogError(error.GenerateErrorReport());

        string errorReport = error.GenerateErrorReport();


        ErrorReport.gameObject.SetActive(true);
        var result = errorReport.Substring(errorReport.LastIndexOf('\n') + 1);
        ErrorReport.text = "";
        await UniTask.Delay(TimeSpan.FromSeconds(.5f));


        ErrorReport.text = result;

        //loginEvent = false;



        /*var registerNewUser = new RegisterPlayFabUserRequest {Password = _userPassword, Username = _userName , Email = _userEmail};

        PlayFabClientAPI.RegisterPlayFabUser(registerNewUser, OnRegisterSuccess, OnRegisterFailure);*/
    }

    private async void SwitchPanel()
    {

    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");

        PlayerPrefs.SetString("LoginEmail", _userEmail);
        PlayerPrefs.SetString("LoginPassword", _userPassword);
        PlayerPrefs.SetString("LoginName", _userName);
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = _userName },
            OnDisplayName, OnLoginFailure);

        SignInOption.SetActive(false);
        //LoginPanel.SetActive(false);
        StartingPanel.SetActive(true);
        GetStats();
        GetHighScore();
        PanelData.Instance.ShowLoginPanel = false;


    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your new display name");
    }

    private async void OnRegisterFailure(PlayFabError error)
    {
        string errorReport = error.GenerateErrorReport();
        

        ErrorReport.gameObject.SetActive(true);
        var result = errorReport.Substring(errorReport.LastIndexOf('\n') + 1);
        if (result.Contains("Email address"))
        {
            Debug.Log("hello error");
             result = result.Replace("Email address", "Username");
        }
        Debug.Log("Register error : " +result);
        ErrorReport.text = "";
        await UniTask.Delay(TimeSpan.FromSeconds(.5f));


        ErrorReport.text = result;

        /*string[] singleErrorReport = errorReport.Split('\n');
        foreach (string sub in singleErrorReport)
        {
            Debug.Log(sub);
        }*/
    }

    public void SetUserEmail(string email)
    {
        _userEmail = email;
    }

    public void SetUserPassWord(string password)
    {
        _userPassword = password;
        
    }

    public void SetConfirmUserPassWord(string password)
    {
        _confirmUserPassword = password;

    }

    public void SetUserName(string uName)
    {
        _userName = uName;
        SetUserEmail(_userName+"email@gmail.com" );
    }

    private string GetUserEmail()
    {
        return _userEmail;
    }

    private string GetUserPassword()
    {
        return _userPassword;
    }

    private string GetUserName()
    {
        return _userName;
    }

    public void OnClickLogin()
    {

        /*var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);*/


        if (loginEvent)
        {
            var request = new LoginWithPlayFabRequest { Username = _userName, Password = _userPassword };
            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
        }
        else
        {

            if (_userPassword == _confirmUserPassword)
            {
                var registerNewUser = new RegisterPlayFabUserRequest { Password = _userPassword, Username = _userName, Email = _userEmail };

                PlayFabClientAPI.RegisterPlayFabUser(registerNewUser, OnRegisterSuccess, OnRegisterFailure);

            }
            else
            {
                ErrorReport.gameObject.SetActive(true);



                ErrorReport.text = "Passwords do not match";

            }


        }



    }


    public void SetLoginEvent(bool login)
    {
        loginEvent = login;
    }

    public void CleanData()
    {
        _userName = "";
        _userPassword = "";
        _userEmail = "";
        _confirmUserPassword = "";
    }


    #endregion



    #region Upload_Data_To_Cloud

    public void SetPlayerData(int score)
    {
        PlayerHighScore = score;
        PlayerCompletedLevel = PlayerPrefs.GetInt("SetHighestStage");
        StartCloudUpdatePlayerStats();
    }


    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                    new StatisticUpdate { StatisticName = "PlayerCompletedLevel", Value = PlayerCompletedLevel },
                    new StatisticUpdate { StatisticName = "PlayerHighScore", Value = PlayerHighScore },
                    new StatisticUpdate { StatisticName = "PlayerId", Value = PlayerId },
                }
        },
            result => { Debug.Log("User statistics updated"); },
            error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {


        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);

            switch (eachStat.StatisticName)
            {
                case "PlayerCompletedLevel":
                    PlayerCompletedLevel = eachStat.Value;
                    PlayerPrefs.SetInt("SetHighestStage", PlayerCompletedLevel);
                    Debug.Log("current player level : " + PlayerCompletedLevel);

                    break;

                case "PlayerHighScore":
                    PlayerHighScore = eachStat.Value;
                    PlayerPrefs.SetInt("HighScore", PlayerHighScore);
                    Debug.Log("current player highscore : "+ PlayerHighScore);
                    break;

                case "PlayerId":
                    PlayerId = eachStat.Value;
                    break;
            }
        }

    }


    public void StartCloudUpdatePlayerStats()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats",
            FunctionParameter = new { _playerCompletedLevel = PlayerCompletedLevel, _playerHighScore = PlayerHighScore, _playerId = PlayerId },
            GeneratePlayStreamEvent = true,
        }, OnCloudUpdateStats, OnErrorShared);
    }

    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        //Debug.Log(JsonWrapper.SerializeObject(result.FunctionResult));
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue);
        Debug.Log((string)messageValue);
    }

    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    #endregion



    #region LeaderBoard


    #region HighScoreLeaderBoard

    public void GetLeaderBoardHighScore()
    {
        CleanLeaderBoard();
        var requestLeaderBoard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerHighScore", MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderBoard, onGetLeaderBoardHighScore, OnErrorLeaderBoard);
    }

    void onGetLeaderBoardHighScore(GetLeaderboardResult result)
    {
        StartingPanel.SetActive(false);
        LeaderBoardPanel.SetActive(true);
        int i = 1;

        TotalHighScore = result.Leaderboard[0].StatValue;
        Debug.Log("1st postion player score " + result.Leaderboard[0].StatValue);


        Debug.Log("Called LeaderBoard");

        foreach (PlayerLeaderboardEntry player in result.Leaderboard)
        {

            GameObject instantiateRow = Instantiate(LeaderBoardRowPrefab, LeaderBoardContainer.transform);

            LeaderBoard ld = instantiateRow.GetComponent<LeaderBoard>();

            //ld.PlayerRank.text = i.ToString("000");
            ld.PlayerName.text = player.DisplayName;
            ld.PlayerScore.text = player.StatValue.ToString("00000");

            Debug.Log(player.DisplayName + ": " + player.StatValue);

            i++;
        }
    }


    public void GetHighScore()
    {
        var requestLeaderBoard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerHighScore", MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderBoard, OnGetHighestScore, OnErrorLeaderBoard);

    }

    void OnGetHighestScore(GetLeaderboardResult result)
    {
        TotalHighScore = result.Leaderboard[0].StatValue;

        LeaderBoardHighScore?.Invoke();
    }

    #endregion

    #region LevelCompletedLeaderBoard


    public void GetLeaderBoardLevel()
    {
        CleanLeaderBoard();
        var requestLeaderBoard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerCompletedLevel", MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderBoard, onGetLeaderBoardLevel, OnErrorLeaderBoard);
    }

    void onGetLeaderBoardLevel(GetLeaderboardResult result)
    {
        
        int i = 1;

        Debug.Log("Called LeaderBoard Level");

        foreach (PlayerLeaderboardEntry player in result.Leaderboard)
        {

            GameObject instantiateRow = Instantiate(LeaderBoardRowPrefab, LeaderBoardContainer.transform);

            LeaderBoard ld = instantiateRow.GetComponent<LeaderBoard>();

            //ld.PlayerRank.text = i.ToString("000");
            ld.PlayerName.text = player.DisplayName;
            ld.PlayerScore.text = player.StatValue.ToString("00000");

            Debug.Log(player.DisplayName + ": " + player.StatValue);

            i++;
        }
    }


    #endregion


    void OnErrorLeaderBoard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void CloseLeaderBoard()
    {
        LeaderBoardPanel.SetActive(false);

        StartingPanel.SetActive(true);

        Debug.Log("Calling LeaderBoard Close");

        CleanLeaderBoard();
    }

    public void CleanLeaderBoard()
    {
        for (int i = LeaderBoardContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(LeaderBoardContainer.transform.GetChild(i).gameObject);
        }
    }
    #endregion



}