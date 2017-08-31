using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class RLPSOUIController : MonoBehaviour
{
	public 	RectTransform    	menu_panel          = null;
    public 	RectTransform    	training_panel      = null;
	private RLPSO 				experiment			= null;

	private void Start()
	{
		experiment = GetComponent<RLPSO>();
	}

    public void MaxParticle(float f)
    {
        experiment.max_particles = Mathf.RoundToInt(f);
    }

    public void ParticleStep(float f)
    {
        experiment.next_particle_wait = Mathf.RoundToInt(f);
    }

    public void BackButton()
    {
        SceneManager.LoadScene(0);
    }

    public void SpeedMultiplier(float f)
    {
        Time.timeScale = f;
    }

	private void Update()
    {
        if (experiment.abort_learning)
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
                new Vector3(0, 480, 0),
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