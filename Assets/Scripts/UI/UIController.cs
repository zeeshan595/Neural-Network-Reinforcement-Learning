using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public RectTransform    menu_panel          = null;
    public RectTransform    training_panel      = null;
    public Experiment       experiment          = null;

    private void Update()
    {
        if (!experiment.session_activated)
        {
            menu_panel.localPosition        = Vector3.Lerp(
                menu_panel.localPosition,
                new Vector3(0, 0, 0),
                Time.deltaTime * 4.0f
            );
            training_panel.localPosition    = Vector3.Lerp(
                training_panel.localPosition,
                new Vector3(-250, 0, 0),
                Time.deltaTime * 4.0f
            );
        }
        else
        {
            menu_panel.localPosition        = Vector3.Lerp(
                menu_panel.localPosition,
                new Vector3(0, 400, 0),
                Time.deltaTime * 4.0f
            );
            training_panel.localPosition    = Vector3.Lerp(
                training_panel.localPosition,
                new Vector3(0, 0, 0),
                Time.deltaTime * 4.0f
            );
        }
    }
}