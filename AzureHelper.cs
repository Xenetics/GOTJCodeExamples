using UnityEngine;
using System.Runtime.InteropServices;

public class AzureHelper : MonoBehaviour
{
    // Name of Azure storage account
    [HideInInspector]
    public const string AccountName = "gotjreplay";
    // Primary connection key Azure storage account
    [HideInInspector]
    public const string ConnectionKey = "Removed";

    /// <summary> Sends a connection request azure though the dll </summary>
    /// <param name="Storage_Account"> Storage Account name </param>
    /// <param name="Connection_Key"> String key for account </param>
    /// <returns> A bool for connected or not </returns>
    [DllImport("AzureGOTJ")]
    public static extern bool Connect(string Storage_Account, string Connection_Key);

    /// <summary> Creates a Replay on Azure </summary>
    /// <param name="Replay_ID"> Identifier for the replay </param>
    /// <param name="Replay_Data"> Data for the replay system to use </param>
    /// <returns> A bool for connected or not </returns>
    [DllImport("AzureGOTJ")]
    public static extern bool CreateReplay(string Replay_ID, string Replay_Data);

    /// <summary> Deletes a Replay on Azure </summary>
    /// <param name="Replay_ID"> Identifier for the replay </param>
    /// <returns> A bool for connected or not </returns>
    [DllImport("AzureGOTJ")]
    public static extern bool DeleteReplay(string Replay_ID);

    /// <summary> Retrieves a Replay from Azure in string </summary>
    /// <param name="Replay_ID"> ID of the replay you would like to retrieve </param>
    /// <returns> A string of data for the replay </returns>
    [DllImport("AzureGOTJ")]
    public static extern string RetrieveReplay(string Replay_ID);

    /// <summary> Retrieve a list of replays in the amount of range starting in the replays at start point given </summary>
    /// <param name="Start_Point"> Place in the table call results to start </param>
    /// <param name="Range"> how many replays to call down </param>
    /// <returns> A @ delimited list of replay identifiers </returns>
    [DllImport("AzureGOTJ")]
    public static extern string RetrieveRangeOfReplays(int Start_Point, int Range);

    /// <summary> Retrieves a list of all replays in azure 
    /// !!!CAUTION!!! This is a bad Idea </summary>
    /// <returns> A @ delimited list of replay identifiers </returns>
    [DllImport("AzureGOTJ")]
    public static extern string RetrieveListOfReplays();

    /// <summary> Adds a like to a replay </summary>
    /// <param name="Replay_ID"> The ID of the replay you would like to increase the like on </param>
    /// <returns> Returns a true if task is completed </returns>
    [DllImport("AzureGOTJ")]
    public static extern bool LikeReplay(string Replay_ID);

    //void Start()
    //{
    //    Debug.Log(Connect(AccountName, ConnectionKey));
    //    Debug.Log(CreateReplay("Testies012345678910", "1098142825%10.64814$-0.1,7.5#0.0,0.0#"));
    //    Debug.Log(DeleteReplay("Testies012345678910"));
    //}
}
