using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Google ARCore Extensions
using Google.XR.ARCoreExtensions;
using System;

public class CloudAnchorManager : MonoBehaviour
{
    // 상태 변수
    public enum Mode { READY, HOST, HOST_PENDING, RESOLVE, RESOLVE_PENDING };

    // 버튼
    public Button hostButton;       // 클라우드 앵커 등록
    public Button resolveButton;    // 클라우드 앵커 조회
    public Button resetButton;      // 리셋

    // 메시지 출력 텍스트
    public Text messageText;

    // 상태 변수
    public Mode mode = Mode.READY;
    // AnchorManager //로컬 앵커를 생성하기 위한 클래스
    public ARAnchorManager anchorManager;
    // ArRaycastManager
    public ARRaycastManager raycastManager;

    // 증강시킬 객체 프리팹
    public GameObject anchorPrefab;
    // 저장 객체 변수(삭제하기 위한 용도)
    private GameObject anchorGameObject;

    // 로컬앵커 저장 변수
    private ARAnchor localAnchor;
    // 클라우드 앵커 변수
    private ARCloudAnchor cloudAnchor;


    // Raycast Hit
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // 클라우드 앵커 ID 저장하기 위한 키값 (PlayerPrefs.SetString("키", 값))
    private const string cloudAnchorKey = "CLOUD_ANCHOR_ID";
    // 클라우드 앵커 ID
    private string strCloudAnchorId;

    void Start()
    {
        // 버튼 이벤트 연결
        hostButton.onClick.AddListener(() => OnHostClick());
        resolveButton.onClick.AddListener(() => OnResolveClick());
        resetButton.onClick.AddListener(() => OnResetClick());
    }

    void Update()
    {
        if (mode == Mode.HOST)
        {
            Hosting();
            HostProcessing();
        }
    }

    void Hosting()
    {
        // 탭 없을 경우 리턴
        if (Input.touchCount < 1) return;

        // 첫 번째 터치가 아닐 경우 리턴
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // 로컬 앵커가 존재하는지 여부를 확인
        if (localAnchor == null)
        {
            // Raycast 발사
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                // 로컬 앵커를 생성
                localAnchor = anchorManager.AddAnchor(hits[0].pose);
                // 로컬 앵커 위치에 Mummy 증강시키고 변수에 저장
                anchorGameObject = Instantiate(anchorPrefab, localAnchor.transform);
            }
        }
    }

    // 클라우드 앵커 등록
    void HostProcessing()
    {
        // 로컬 앵커가 생성되지 않았을 때 리턴
        if (localAnchor == null) return;

        // 피쳐 포인트의 갯수 많을 수록 맵핑 퀄리티가 증가함.
        /*
            Insufficient = 0 // 불충분
            Sufficient   = 1 // 충분
            Good         = 1 // Good
        */
        FeatureMapQuality quality
            = anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());

        string mappingText = string.Format("맵핑 품질 = {0}", quality);

        // 맵핑 퀄리티가 1이상일 때 호스팅 요청
        if (quality == FeatureMapQuality.Sufficient || quality == FeatureMapQuality.Good)
        {
            cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);

            if (cloudAnchor == null)
            {
                mappingText = "클라우드 앵커 생성 실패";
            }
            else
            {
                mappingText = "클라우드 앵커 생성 시작";
                mode = Mode.HOST_PENDING;
            }
        }

        messageText.text = mappingText;
    }

    // Maincamera 태그로 지정된 카메라의 위치와 각도를 Pose 데이터 타입으로 반환
    public Pose GetCameraPose()
    {
        return new Pose(Camera.main.transform.position, Camera.main.transform.rotation);
    }

    void OnHostClick()
    {
        mode = Mode.HOST;
    }

    void OnResolveClick()
    {
        mode = Mode.RESOLVE;
    }

    void OnResetClick()
    {
    }



}
