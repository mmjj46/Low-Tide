using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(RawImage))]
public class WallEraser : MonoBehaviour
{
    [Header("균열 설정")]
    public Texture2D crackSourceImage;
    public bool isMyTurn = false;

    [Header("수리 도구(브러시) & 카메라")]
    public int brushSize = 30;
    public Camera uiCamera;

    [Header("수리 완료 조건")]
    [Range(0.01f, 1.0f)]
    public float clearThreshold = 0.05f;

    [Header("이벤트")]
    public UnityEvent onThisCrackRepaired;

    private RawImage crackImage;
    private Texture2D editableCrackTexture;
    private Color32[] pixelData;
    private bool isDirty = false;
    private RectTransform imageRectTransform;
    private Color32 clearColor = new Color32(0, 0, 0, 0);

    private int totalCrackPixels;
    private int remainingCrackPixels;
    public bool isRepaired = false;

    private Vector2Int lastDrawPosition;
    private bool isDrawing = false;
    private int textureWidth;
    private int textureHeight;

    void Awake()
    {
        crackImage = GetComponent<RawImage>();
        imageRectTransform = GetComponent<RectTransform>();

        // 1. 강제로 화면 맨 앞으로 가져오기 (배경에 가려짐 방지)
        transform.SetAsLastSibling();

        // 2. 투명도 및 색상 초기화 (안 보이는 현상 방지)
        crackImage.color = Color.white;
        crackImage.raycastTarget = true;
    }

    void Start()
    {
        if (crackSourceImage == null)
        {
            // 인스펙터에 이미지가 안 들어가 있으면 기존 텍스처라도 유지
            if (crackImage.texture != null)
            {
                crackSourceImage = (Texture2D)crackImage.texture;
            }
            else
            {
                return;
            }
        }

        InitializeCrack();
    }

    private void InitializeCrack()
    {
        try
        {
            // 원본 데이터 복사
            pixelData = crackSourceImage.GetPixels32();
            textureWidth = crackSourceImage.width;
            textureHeight = crackSourceImage.height;

            // 편집용 새 텍스처 생성
            editableCrackTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            editableCrackTexture.SetPixels32(pixelData);
            editableCrackTexture.Apply();

            // 이미지 연결
            crackImage.texture = editableCrackTexture;

            // 픽셀 통계
            totalCrackPixels = 0;
            for (int i = 0; i < pixelData.Length; i++)
            {
                if (pixelData[i].a > 10) totalCrackPixels++;
            }
            remainingCrackPixels = totalCrackPixels;
            isDirty = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{gameObject.name}: 픽셀 데이터 접근 불가! 이미지의 'Read/Write Enabled'가 켜져 있는지 확인하세요. {e.Message}");
        }
    }

    void Update()
    {
        if (!isMyTurn || isRepaired || Time.timeScale == 0) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            Vector2Int currentPixel = GetCurrentPixelPosition();
            if (currentPixel.x != -1)
            {
                DrawCircle(currentPixel);
                lastDrawPosition = currentPixel;
            }
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2Int currentPixel = GetCurrentPixelPosition();
            if (currentPixel.x != -1 && currentPixel != lastDrawPosition)
            {
                DrawLine(lastDrawPosition, currentPixel);
                lastDrawPosition = currentPixel;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            CheckCompletion();
        }
    }

    void LateUpdate()
    {
        if (isDirty)
        {
            editableCrackTexture.SetPixels32(pixelData);
            editableCrackTexture.Apply();
            isDirty = false;
        }
    }

    private Vector2Int GetCurrentPixelPosition()
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRectTransform, Input.mousePosition, uiCamera, out localPoint))
        {
            float uvX = (localPoint.x + imageRectTransform.rect.width * imageRectTransform.pivot.x) / imageRectTransform.rect.width;
            float uvY = (localPoint.y + imageRectTransform.rect.height * imageRectTransform.pivot.y) / imageRectTransform.rect.height;

            if (uvX < 0 || uvX > 1 || uvY < 0 || uvY > 1) return new Vector2Int(-1, -1);

            int pixelX = Mathf.Clamp((int)(uvX * textureWidth), 0, textureWidth - 1);
            int pixelY = Mathf.Clamp((int)(uvY * textureHeight), 0, textureHeight - 1);
            return new Vector2Int(pixelX, pixelY);
        }
        return new Vector2Int(-1, -1);
    }

    private void DrawLine(Vector2Int from, Vector2Int to)
    {
        float distance = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, (int)(distance / 2f));
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 lerped = Vector2.Lerp(from, to, t);
            DrawCircle(new Vector2Int(Mathf.RoundToInt(lerped.x), Mathf.RoundToInt(lerped.y)));
        }
    }

    private void DrawCircle(Vector2Int center)
    {
        int r = brushSize;
        int rSq = r * r;
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y > rSq) continue;
                int px = center.x + x;
                int py = center.y + y;
                if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                {
                    int idx = py * textureWidth + px;
                    if (pixelData[idx].a > 0)
                    {
                        pixelData[idx] = clearColor;
                        remainingCrackPixels--;
                        isDirty = true;
                    }
                }
            }
        }
    }

    private void CheckCompletion()
    {
        if (totalCrackPixels == 0) return;
        if ((float)remainingCrackPixels / totalCrackPixels < clearThreshold)
        {
            isRepaired = true;
            onThisCrackRepaired.Invoke();
            // 완전히 지워진(수리된) 상태 유지
            crackImage.color = new Color(1, 1, 1, 0);
        }
    }
}