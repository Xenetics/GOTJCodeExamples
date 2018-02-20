using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Date Created: 09/28/15 ->
/// Handles the camera
/// </summary>
public class CameraSystem : MonoBehaviour
{
    private static CameraSystem instance = null;
    public static CameraSystem Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        //#if UNITY_IPHONE || UNITY_ANDROID
        //if (((float)Screen.height / (float)Screen.width) > (16f/9f))
        //{
        //    Camera.main.orthographicSize = Camera.main.orthographicSize * (((float)Screen.height / (float)Screen.width) / (3f / 2f)); //was 3/2 previously
        //}
        //#endif
        Camera.main.orthographicSize = Camera.main.orthographicSize * ((16f / 9f) / (3f / 2f));
    }
    /// <summary> Enum with the 2 camera types </summary>
    public enum CameraMode { Track, Attached, Cinematic, Zoom, Restart }
    /// <summary> This is what camera mode we are currently in </summary>
    [SerializeField]
    private CameraMode m_CurCameraMode = CameraMode.Track;
    /// <summary> CAmeras start point after restart </summary>
    private Vector2 CameraOrigin = new Vector2(-11f, 2.5f);
    /// <summary> The normal cam zoom level </summary>
    private float m_NormZoom;
    /// <summary> How many levels of zoom to have total </summary>
    [SerializeField]
    private int m_LevelsOfZoom = 5;
    /// <summary> How much to zoom in per level of zoom </summary>
    private float m_ZoomPerLevel;
    /// <summary> The level of zoom we are at </summary>
    private float m_ZoomLevel = 0;
    /// <summary> The level of zoom to use instantly befor lerping </summary>
    [SerializeField]
    private float m_InstantZoomTo = 1.0f;
    /// <summary> Wheather the camera will lerp to zoom point or instant to a certain point </summary>
    private bool m_Zooming = false;
    /// <summary> The position in the list of Object we want to look at</summary>
    private int m_PositionIndex = 0;
    /// <summary> The amount of branchaes that the camera will let George get ahead </summary>
    [SerializeField]
    private int m_BranchThreshold = 2;
    /// <summary> The offset on the screen the camera will sit from george </summary>
    [SerializeField]
    private float m_GeorgeOffset = 4;
    /// <summary> The speed at which the camera lerps  </summary>
    [SerializeField]
    private float m_Speed = 2;
    /// <summary> The speed at which the camera zooms </summary>
    public float m_ZoomSpeed = 2;
    /// <summary> Refference to the player object </summary>
    private GameObject m_Player;
    /// <summary> Reference to Georges center point </summary>
    private GameObject m_PlayerCenter;
	///<summary> position for the cinematic camera </summary>
	public Vector2 CinematicTarget;
    /// <summary> Cinematic zoom level </summary>
    [SerializeField]
    private float m_CinematicZoom = 1.0f;
    /// <summary> cinematic pan speed </summary>
    public float m_CinematicSpeed = 20.0f;
    /// <summary> Whether the camera is shaking or not </summary>
    private bool m_IsShaking = false;
    /// <summary> Camera Shake Intensity </summary>
    [SerializeField]
    private float m_ShakeIntensity = 1.0f;
    /// <summary> Time to shake for </summary>
    [SerializeField]
    private float m_ShakeTime = 0.5f;
    /// <summary> Timer for shaking </summary>
    private float m_ShakeTimer;
    /// <summary> Callback function delegate </summary>
    /// <param name="mode"> camera mode </param>
    public delegate void CallBack(CameraMode mode);
    /// <summary> The function we ill be calling </summary>
    private CallBack m_MyCallBack;
    /// <summary> CAmeramode to swap to after call back </summary>
    private CameraMode m_ModeFromCallback;
    public bool fullyAtatched = false;
    public float minCamY = 0;

    void Start()
    {
        m_Player = GameManager.Instance._playerState.gameObject;
        m_PlayerCenter = GameObject.Find("GeorgeCenterPoint");       

        m_NormZoom = Camera.main.orthographicSize;
        m_ZoomPerLevel = (float)System.Math.Round(m_NormZoom / m_LevelsOfZoom, 2);
        m_ShakeTimer = m_ShakeTime;
    }

    public Vector2 GetOrigin()
    {
        return CameraOrigin;
    }

    void Update()
    {
        ShakeBehaviour();
    }

    void LateUpdate()
    {
        HandleState();
    }

    /// <summary> Handles state update calls </summary>
    private void HandleState()
    {
        switch (m_CurCameraMode)
        {
            case CameraMode.Attached:
                AttachedMode();
                break;
            case CameraMode.Cinematic:
                CinematicMode();
                break;
            case CameraMode.Track:
                TrackMode();
                break;
            case CameraMode.Zoom:
                ZoomMode();
                break;
            case CameraMode.Restart:
                RestartMode();
                break;
        }
    }

    /// <summary> Called when a new state is entered </summary>
    private void OnStateEnter()
    {
        switch (m_CurCameraMode)
        {
            case CameraMode.Attached:
                
                break;
            case CameraMode.Cinematic:
                //Camera.main.orthographicSize = (m_NormZoom * m_CinematicZoom);
                break;
            case CameraMode.Track:
                
                break;
            case CameraMode.Zoom:
                
                break;
            case CameraMode.Restart:

                break;
        }
    }

    /// <summary> Called when a state is exited </summary>
    private void OnStateExit()
    {
        switch (m_CurCameraMode)
        {
            case CameraMode.Attached:
                
                break;
            case CameraMode.Cinematic:
                
                break;
            case CameraMode.Track:
                
                break;
            case CameraMode.Zoom:
                  
                break;
            case CameraMode.Restart:

                break;
        }
    }

    /// <summary> Shake the camera for when george hits </summary>
    private void ShakeBehaviour()
    {
        if (m_IsShaking)
        {
            m_ShakeTimer -= Time.unscaledDeltaTime;
            Vector2 shakeTo = (Random.insideUnitCircle * m_ShakeIntensity);
            Camera.main.transform.position = new Vector3(shakeTo.x, Camera.main.transform.position.y + shakeTo.y, Camera.main.transform.position.z);
            if (m_ShakeTimer <= 0)
            {
                m_IsShaking = false;
                m_ShakeTimer = m_ShakeTime;
            }
        }
    }

    /// <summary> Sets the camera to shake </summary>
    public void ShakeCam()
    {
        m_IsShaking = true;
    }

    /// <summary> Sets the camera to shake and how much and for how long </summary>
    /// <param name="intensity"> Intensity of shake </param>
    /// <param name="length"> How long to shake for </param>
    public void ShakeCam(float intensity, float length)
    {
        m_ShakeIntensity = intensity;
        m_ShakeTime = length;
        m_IsShaking = true;
    }

    /// <summary> Attaches the camera to player </summary>
    private void AttachedMode()
    {
        if (Mathf.Abs(gameObject.transform.position.y - m_PlayerCenter.transform.position.y) > 0.4f && !fullyAtatched)
        {
            Vector2 newPos = new Vector2(m_PlayerCenter.transform.position.x, m_PlayerCenter.transform.position.y);
            Vector2 updatePos = Vector2.Lerp(gameObject.transform.position, newPos, m_Speed * Time.deltaTime);
            gameObject.transform.position = new Vector3(0, Mathf.Clamp(updatePos.y, minCamY,Mathf.Infinity), gameObject.transform.position.z);
            //fullyAtatched = false;
        }
        else
        {
            gameObject.transform.position = new Vector3(0, Mathf.Clamp (m_PlayerCenter.transform.position.y, minCamY, Mathf.Infinity), gameObject.transform.position.z);
            gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            //fullyAtatched = true;
        }
    }

    /// <summary> Zooms to a given point </summary>
    private void CinematicMode()
    {
        if(Vector2.Distance(gameObject.transform.position, CinematicTarget) > 0.01f)
        {
            Vector2 updatePos = Sinerp(gameObject.transform.position, CinematicTarget, m_CinematicSpeed * Time.deltaTime);
            gameObject.transform.position = new Vector3(updatePos.x, updatePos.y, gameObject.transform.position.z);
        }
        else
        {
            gameObject.transform.position = new Vector3(CinematicTarget.x, CinematicTarget.y, gameObject.transform.position.z);
            gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            if(m_MyCallBack != null)
            {
                m_MyCallBack(CameraMode.Track);
            }
        }
    }

    /// <summary> Tracks player but lerps behind </summary>
    private void TrackMode()
    {
        Vector2 newPos = new Vector2(m_Player.transform.position.x, m_Player.transform.position.y + m_GeorgeOffset);
        Vector2 updatePos = Sinerp(gameObject.transform.position, newPos, m_Speed * Time.deltaTime);
        gameObject.transform.position = new Vector3(0, updatePos.y, gameObject.transform.position.z);
        gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    /// <summary> Zooms in on the player based on zoom settings </summary>
    private void ZoomMode()
    {
        if (Vector2.Distance(gameObject.transform.position, m_PlayerCenter.transform.position) > 1.5f)
        {
            Vector2 updatePos = Sinerp(gameObject.transform.position, m_Player.transform.position, m_Speed * Time.deltaTime);
            gameObject.transform.position = new Vector3(updatePos.x, updatePos.y, gameObject.transform.position.z);
        }
        else
        {
            gameObject.transform.position = new Vector3(m_PlayerCenter.transform.position.x, m_PlayerCenter.transform.position.y, gameObject.transform.position.z);
            gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            if (!m_Zooming)
            {
                Camera.main.orthographicSize = (m_NormZoom - (m_ZoomLevel * m_ZoomPerLevel)) + (m_ZoomPerLevel - (m_ZoomPerLevel * m_InstantZoomTo));
                m_Zooming = true;
            }
            else
            {
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, m_NormZoom - (m_ZoomLevel * m_ZoomPerLevel), m_ZoomSpeed * Time.deltaTime);
            }
        }
    }

    private void RestartMode()
    {
        gameObject.transform.position = new Vector3(CameraOrigin.x, CameraOrigin.y, transform.position.z);
        if (m_MyCallBack != null && Vector2.Distance(GameManager.Instance._playerState.transform.position, CameraOrigin) < 5f)
        {
            m_MyCallBack(CameraMode.Track);
        }
    }

    /// <summary> Sets the mode of the camera </summary>
    /// <param name="mode"> Cameramode to set to </param>
    public void SetMode(CameraMode mode)
	{
        OnStateExit();
		m_CurCameraMode = mode;
        OnStateEnter();
    }

    /// <summary>  Sets the mode of the camera  </summary>
    /// <param name="mode"> Cameramode to set to </param>
    public static void SetModeStat(CameraMode mode)
    {
        Instance.OnStateExit();
        Instance.m_CurCameraMode = mode;
        Instance.OnStateEnter();
    }

    /// <summary> Sets the mode of the camera  </summary>
    /// <param name="mode"> Cameramode to set to </param>
    /// <param name="pos"> position to go to </param>
    public void SetMode(CameraMode mode, Vector2 pos)
	{
        OnStateExit();
		CinematicTarget = pos;
		m_CurCameraMode = mode;
        OnStateEnter();
    }

    /// <summary> Sets the mode of the camera </summary>
    /// <param name="mode"> Cameramode to set to </param>
    /// <param name="pos"> position to go to </param>
    /// <param name="function"> function to call back </param>
    /// <param name="mode2"> Cameramode to set to with callback</param>
    public void SetMode(CameraMode mode, Vector2 pos, CallBack function, CameraMode mode2)
    {
        OnStateExit();
        CinematicTarget = pos;
        m_CurCameraMode = mode;
        m_MyCallBack = function;
        m_ModeFromCallback = mode2;
        OnStateEnter();
    }

    /// <summary> Increases level of zoom </summary>
    public void ZoomIn()
    {
        if (m_ZoomLevel < m_LevelsOfZoom - 1)
        {
            m_ZoomLevel += 1;
            m_Zooming = false;
        }
    }

    /// <summary> Lowers level of zoom </summary>
    public void ZoomOut()
    {
        if (m_ZoomLevel > 0)
        {
            m_ZoomLevel -= 1;
            m_Zooming = false;
        }
    }

    /// <summary> Reset zoom level </summary>
    public void ResetZoom()
    {
        m_ZoomLevel = 0;
    }

    /// <summary>
    /// Snaps the cam to a given item
    /// </summary>
    /// <param name="pos"></param>
    public void SnapTo(Vector3 pos)
    {
        gameObject.transform.position = new Vector3(pos.x, pos.y, gameObject.transform.position.z);
    }

    /// <summary> Lerps from one Vector to another easing from slow to fast then to slow </summary>
    /// <param name="start">Current camera position</param>
    /// <param name="end">Desired camera position</param>
    /// <param name="value">General speed at which to lerp</param>
    /// <returns> The new position to move to </returns>
    private Vector2 Sinerp(Vector2 start, Vector2 end, float value)
    {
        return Vector2.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
    }

    /// <summary> gets the current cam state </summary>
    /// <returns> what state the camera is in </returns>
    public CameraMode GetState()
    {
        return m_CurCameraMode;
    }
}