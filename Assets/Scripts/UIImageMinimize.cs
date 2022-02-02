using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class UIImageMinimize : MaskableGraphic
{
    /// <summary>
    /// Загруженная текстура
    /// </summary>
    [SerializeField]
    public Texture texture;

    [SerializeField, Range(0, 1)]
    private float Ywarping;

    [SerializeField, Range(0, 1)]
    private float Xwarping;

    public override Texture mainTexture
    {
        get { return texture; }
    }

    [SerializeField]
    private int xSegmentsNumber;

    [SerializeField]
    private int ySegmentsNumber;

    [SerializeField]
    private AnimationCurve CurveY = new AnimationCurve();

    [SerializeField]
    private float ForceCurve = 150f;

    /// <summary>
    /// массив векторов2 ВСЕ ТОЧКИ
    /// </summary>
    private Vector2[] vertices;    //ВОТ ЭТО МЕНЯЕТСЯ ПРИ АНИМАЦИИ

    /// <summary>
    /// массив векторов2 ВСЕ ТОЧКИ
    /// </summary>
    private Vector3[] verticesVisualize;

    private bool isInitVertices = false;

    private Coroutine coroutine;

    protected override void OnPopulateMesh(VertexHelper vertexHelper) //заполнить сетку ФИГАЧИТ В КАЖДОМ КАДРЕ и обновляет размер сетки
    {
        float minX = (0f - rectTransform.pivot.x) * rectTransform.rect.width; //координаты углов экрана
        float minY = (0f - rectTransform.pivot.y) * rectTransform.rect.height;
        float maxX = (0f + rectTransform.pivot.x) * rectTransform.rect.width;
        float maxY = (0f + rectTransform.pivot.y) * rectTransform.rect.height;
        
        var color32 = (Color32)color;

        if (xSegmentsNumber < 1) xSegmentsNumber = 1;
        if (ySegmentsNumber < 1) ySegmentsNumber = 1; //можно ограничить просто в инспекторе

        vertexHelper.Clear(); //почистили старый

        if (isInitVertices && vertices.Length != (xSegmentsNumber + 1) * (ySegmentsNumber + 1))
        {
            isInitVertices = false;
        }

        if (!isInitVertices)
        {
            verticesVisualize = new Vector3[(xSegmentsNumber + 1) * (ySegmentsNumber + 1)];
            vertices = new Vector2[(xSegmentsNumber + 1) * (ySegmentsNumber + 1)];
        }

        Vector2[] uv = new Vector2[vertices.Length];

        for (int i = 0, y = 0; y <= ySegmentsNumber; y++)
        {
            for (int x = 0; x <= xSegmentsNumber; x++, i++) //X - строки Y - столбцы I - все вместе
            {                
                float xWarp = - y * Xwarping; //сжатие сверху вниз

                float yWarp = ((-2f / (float)xSegmentsNumber) * x + 1) * (CurveY.Evaluate((1.0f/(float)ySegmentsNumber)*(float)y)  -  1.0f) * (1 - Xwarping); //Сужение по горизонтали, но проходимся по Y 
                yWarp += ((-2f / (float)xSegmentsNumber) * x + 1) * Xwarping / ForceCurve * 10;
                yWarp = yWarp * ForceCurve * Ywarping;      
               
                vertices[i] = new Vector2((float)x, (float)y);     
                
                uv[i] = new Vector2((float)x / xSegmentsNumber, (float)y / ySegmentsNumber);

                //вертисы для визуализации на сцене
                verticesVisualize[i] = new Vector2((minX + ((vertices[i].x + yWarp) * ((maxX - minX) / xSegmentsNumber))), (minY + ((vertices[i].y + xWarp) * ((maxY - minY) / ySegmentsNumber))));
                //а вот эти уже для деформации
                vertexHelper.AddVert(new Vector3((minX + ((vertices[i].x + yWarp) * ((maxX - minX) / xSegmentsNumber))), (minY + ((vertices[i].y + xWarp) * ((maxY - minY) / ySegmentsNumber)))), color32, new Vector2(uv[i].x, uv[i].y));
            }
        }

        int[] triangles = new int[xSegmentsNumber * ySegmentsNumber * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySegmentsNumber; y++, vi++)
        {
            for (int x = 0; x < xSegmentsNumber; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSegmentsNumber + 1;
                triangles[ti + 5] = vi + xSegmentsNumber + 2;
                vertexHelper.AddTriangle(triangles[ti], triangles[ti + 1], triangles[ti + 2]);
                vertexHelper.AddTriangle(triangles[ti + 3], triangles[ti + 4], triangles[ti + 5]);
            }
        }
        isInitVertices = true;
    }

    /// <summary>
    /// Отрисовка сетки с точками в инспекторе
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        float minX = (0f - rectTransform.pivot.x) * rectTransform.rect.width;
        float minY = (0f - rectTransform.pivot.y) * rectTransform.rect.height;
        float maxX = (1f - rectTransform.pivot.x) * rectTransform.rect.width;
        float maxY = (1f - rectTransform.pivot.y) * rectTransform.rect.height;
        float disX = ((maxX - minX) / xSegmentsNumber) * 0.1f;
        float disY = ((maxY - minY) / ySegmentsNumber) * 0.1f;
        float dis = disX > disY ? disY : disX;

        int[] triangles = new int[xSegmentsNumber * ySegmentsNumber * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySegmentsNumber; y++, vi++)
        {
            for (int x = 0; x < xSegmentsNumber; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSegmentsNumber + 1;
                triangles[ti + 5] = vi + xSegmentsNumber + 2;
                Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.6f);
                Vector3 p1 = verticesVisualize[triangles[ti]] + transform.position;
                Vector3 p2 = verticesVisualize[triangles[ti + 1]] + transform.position;
                Vector3 p3 = verticesVisualize[triangles[ti + 2]] + transform.position;
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawSphere(p1, dis);
                Gizmos.DrawSphere(p2, dis);
                Gizmos.DrawSphere(p3, dis);
                p1 = verticesVisualize[triangles[ti + 3]] + transform.position;
                p2 = verticesVisualize[triangles[ti + 4]] + transform.position;
                p3 = verticesVisualize[triangles[ti + 5]] + transform.position;
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawSphere(p1, dis);
                Gizmos.DrawSphere(p2, dis);
                Gizmos.DrawSphere(p3, dis);
            }
        }
    }

    private void Update()
    {
        SetAllDirty();
    }

    public void MinimazeWindow(float time)
    {
        coroutine = StartCoroutine(ValueLerping(0f, 1f, time, coroutine));
    }

    public void MaximizeWindow(float time)
    {
        coroutine = StartCoroutine(ValueLerping(1f, 0f, time, coroutine));
    }

    private IEnumerator ValueLerping(float value, float target, float time, Coroutine coroutine)
    {
        if (coroutine != null) StopCoroutine(coroutine);

        if (value < target)
        {
            yield return new WaitForEndOfFrame();            
            var delta = target - value;
            while (value < target)
            {
                value = value + (delta) / 50f / time;
                if (value > 1) value = 1;
                Ywarping = value;
                Xwarping = value;
                yield return new WaitForSeconds(0.02f);
            }
            
        }
        if (value > target)
        {
            var delta = target - value;
            while (value > target)
            {
                value = value + (delta) / 50f / time;
                if (value < 0) value = 0;
                Ywarping = value;
                Xwarping = value;
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}
