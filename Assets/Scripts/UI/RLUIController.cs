using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RLUIController : MonoBehaviour
{
	public RectTransform    menu_panel          = null;
    public RectTransform    training_panel      = null;
	private CarRL			experiment			= null;

    public void SetMemorySize(float f)
    {
        experiment.memory_size = Mathf.RoundToInt(f);
    }
    public void SetMaxBatchSize(float f)
    {
        experiment.max_batch_size = Mathf.RoundToInt(f);
    }
    public void SetEpsilonIncrement(float f)
    {
        experiment.epsilon_increment = Mathf.Pow(0.1f, f);
    }

    public void SetRewardDecay(float f)
    {
        experiment.reward_decay = f;
    }

    public void SetReactionTime(float f)
    {
        experiment.reaction_time = f;
    }

    public void SetLearnWait(float f)
    {
        experiment.learn_wait = Mathf.RoundToInt(f);
    }

    public void SetReplaceTargetIteration(float f)
    {
        experiment.replace_target_iteration = Mathf.RoundToInt(f);
    }

    public void BackButton()
    {
        SceneManager.LoadScene(0);
    }

    public void SpeedMultiplier(float f)
    {
        Time.timeScale = f;
    }

	private void Start()
	{
		experiment = GetComponent<CarRL>();
	}

	private void Update()
    {
        if (!experiment.is_activated)
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