using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum TransitionType { SAME_SCENE, DIFF_SCENE };

    #region 配置项
    [Header("Portal")]
    public TransitionType type;
    public string portalName;
    public string destnationName;
    public Transform destinationPoint;
    #endregion

    #region 组件变量
    private new Collider collider;
    #endregion

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    void OnEnable()
    {
        SceneController.Instance.RegisterPortal(this);
    }

    void OnDisable()
    {
        SceneController.Instance.RemovePortal(this);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            SceneController.Instance.TransitToDestination(this);
        }
    }
}
