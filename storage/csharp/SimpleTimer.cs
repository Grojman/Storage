/// <summary>
/// Represents a simple, frame-updated timer that can increment or decrement
/// over time using a fixed step or a dynamically retrieved step value.
/// </summary>
public class SimpleTimer
{
    /// <summary>
    /// Defines how the timer progresses over time.
    /// ADD increases the timer value.
    /// SUBTRACT decreases the timer value.
    /// </summary>
    public enum TimerType
    {
        /// <summary>
        /// Defines how the timer progresses over time.
        /// Decreases the timer value.
        /// </summary>
        SUBTRACT,
        /// <summary>
        /// Defines how the timer progresses over time.
        /// Increases the timer value.
        /// </summary>
        ADD
    }

    /// <summary>
    /// Indicates whether the timer is currently running.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Current value of the timer.
    /// </summary>
    public float Timer { get; private set; }

    /// <summary>
    /// Initial value assigned to the timer when it is reset.
    /// </summary>
    public float Start { get; set; }

    /// <summary>
    /// Target value that determines when the timer ends.
    /// </summary>
    public float End { get; set; }

    /// <summary>
    /// If true, the timer restarts automatically after reaching the end value.
    /// Default is false.
    /// </summary>
    public bool Loop { get; set; } = false;

    /// <summary>
    /// Fixed step value applied on each update when StepRetrieval is not used.
    /// </summary>
    public float Step { get; set; }

    /// <summary>
    /// Function used to retrieve a dynamic step value each update.
    /// If provided, it takes priority over Step.
    /// </summary>
    public Func<float> StepRetrieval { get; set; }

    /// <summary>
    /// Determines whether the timer adds or subtracts the step value.
    /// Defaults to ADD.
    /// </summary>
    public TimerType Type { get; set; } = TimerType.ADD;

    /// <summary>
    /// Optional callback invoked when the timer reaches its end value.
    /// </summary>
    public Action OnEnd { get; set; }

    // --------------------------------------------------
    // Constructors
    // --------------------------------------------------

    /// <summary>
    /// Creates a timer using a fixed step value.
    /// </summary>
    /// <param name="start">Initial timer value.</param>
    /// <param name="end">End value that triggers completion.</param>
    /// <param name="step">Fixed step applied on each update.</param>
    /// <param name="loop">Whether the timer should loop after ending.</param>
    /// <param name="onEnd">Optional callback invoked when the timer ends.</param>
    public SimpleTimer(
        float start,
        float end,
        float step,
        bool loop = false,
        Action onEnd = null)
    {
        Start = start;
        End = end;
        Step = step;
        Loop = loop;
        OnEnd = onEnd;

        StepRetrieval = null;
        Timer = Start;
        IsActive = true;
    }

    /// <summary>
    /// Creates a timer using a dynamic step retrieval function.
    /// </summary>
    /// <param name="start">Initial timer value.</param>
    /// <param name="end">End value that triggers completion.</param>
    /// <param name="stepRetrieval">Function that returns the step value per update.</param>
    /// <param name="loop">Whether the timer should loop after ending.</param>
    /// <param name="onEnd">Optional callback invoked when the timer ends.</param>
    public SimpleTimer(
        float start,
        float end,
        Func<float> stepRetrieval,
        bool loop = false,
        Action onEnd = null)
    {
        ArgumentNullException.ThrowIfNull(stepRetrieval);

        Start = start;
        End = end;
        StepRetrieval = stepRetrieval;
        Loop = loop;
        OnEnd = onEnd;

        Step = 0f;
        Timer = Start;
        IsActive = true;
    }

    /// <summary>
    /// Resets the timer to the start value.
    /// Optionally stops the timer.
    /// </summary>
    /// <param name="stop">If true, the timer will be deactivated.</param>
    public void ResetTimer(bool stop = false)
    {
        Timer = Start;
        if (stop)
            IsActive = false;
    }

    /// <summary>
    /// Stops the timer, preventing it from updating.
    /// </summary>
    public void StopTimer()
    {
        IsActive = false;
    }

    /// <summary>
    /// Resumes the timer if it was previously stopped.
    /// </summary>
    public void ResumeTimer()
    {
        IsActive = true;
    }

    /// <summary>
    /// Updates the timer by applying the step value.
    /// Should be called once per frame or tick.
    /// </summary>
    public void Update()
    {
        if (!IsActive)
            return;

        float step = StepRetrieval?.Invoke() ?? Step;

        if (Type == TimerType.ADD)
        {
            Timer += step;
            if (Timer >= End)
            {
                OnEnd?.Invoke();
                ResetTimer(!Loop);
            }
        }
        else
        {
            Timer -= step;
            if (Timer <= End)
            {
                ResetTimer(!Loop);
                OnEnd?.Invoke();
            }
        }
    }
}
