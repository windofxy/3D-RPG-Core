using System;
using UnityEngine;

public class MouseManager : Singleton<MouseManager>
{
    public Texture2D point, doorway, attack, target, arrow;

    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    RaycastHit hitInfo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        // 从摄像机向鼠标位置发射一条射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 如果检测到射线碰撞到物体
        if (Physics.Raycast(ray, out hitInfo))
        {
            // 根据物体类型切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(0, 0), CursorMode.Auto);
                    break;
                case "Attackable":
                    Cursor.SetCursor(attack, new Vector2(0, 0), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        // 如果按下鼠标左键，且射线碰撞到物体
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            GameObject hitGO = hitInfo.collider.gameObject ?? null;
            if (hitGO == null) { }
            // 如果物体标签为地面
            else if (hitGO.CompareTag("Ground"))
            {
                //Debug.Log("Clicked Ground!");
                // 触发鼠标点击事件，参数为碰撞点坐标
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            // 如果物体标签为敌人
            else if (hitGO.CompareTag("Enemy"))
            {
                //Debug.Log("Clicked Enemy!");
                // 触发鼠标点击事件，参数为敌人GameObject
                OnEnemyClicked?.Invoke(hitGO);
            }
            // 如果物品标签为可攻击物
            else if (hitGO.CompareTag("Attackable"))
            {
                // 触发鼠标点击事件，参数为可攻击物GameObject
                OnEnemyClicked?.Invoke(hitGO);
            }
        }
    }
}
