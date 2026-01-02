using UnityEngine;

public class PopupEscapeCloser : MonoBehaviour
{
    private void Start()
    {
        if (!YG.YG2.envir.isDesktop)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        } 
    }  
}
