using System.Numerics;

/// <summary>
/// Represents a generic, extensible timer capable of operating on any numeric-like
/// type that supports addition, subtraction, and comparison.
/// </summary>
/// <typeparam name="TCooldownType">
/// The type used to represent timer values (e.g. float, double, TimeSpan, custom structs).
/// </typeparam>
public class ComplexTimer<TCooldownType>
    where TCooldownType :
        IAdditionOperators<TCooldownType, TCooldownType, TCooldownType>,
        ISubtractionOperators<TCooldownType, TCooldownType, TCooldownType>,
        IComparisonOperators<TCooldownType, TCooldownType, bool>
{
    /// <summary>
    /// Defines whether the timer increments or decrements its value.
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
        /// Decreases the timer value.
        /// </summary>
        ADD
    }

    /// <summary>
    /// Determines how the timer progresses over time.
    /// Defaults to ADD.
    /// </summary>
    public TimerType Type { get; set; } = TimerType.ADD;

    /// <summary>
    /// Indicates whether the timer is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// If true, the timer automatically resets and continues after reaching the end.
    /// Defaults to false.
    /// </summary>
    public bool Loop { get; set; } = false;

    /// <summary>
    /// Value at which the timer is considered finished.
    /// </summary>
    public TCooldownType End { get; set; }

    /// <summary>
    /// Initial value assigned to the timer when started or reset.
    /// </summary>
    public TCooldownType Start { get; set; }

    /// <summary>
    /// Current value of the timer.
    /// </summary>
    public TCooldownType Timer { get; private set; }

    /// <summary>
    /// Fixed step value applied on each update when StepRetrieval is not used.
    /// </summary>
    public TCooldownType Step { get; set; }

    /// <summary>
    /// Function used to retrieve a dynamic step value on each update.
    /// Takes priority over Step when provided.
    /// </summary>
    public Func<TCooldownType> StepRetrieval { get; set; }

    /// <summary>
    /// Invoked when the timer reaches its end value.
    /// </summary>
    public Action OnTimerEnds { get; set; }

    /// <summary>
    /// Invoked when the timer is started or resumed.
    /// </summary>
    public Action OnTimerStarts { get; set; }

    /// <summary>
    /// Invoked when the timer is stopped.
    /// </summary>
    public Action OnTimerStops { get; set; }

    /// <summary>
    /// Invoked when the timer is reset to its start value.
    /// </summary>
    public Action OnTimerResets { get; set; }


    /// <summary>
    /// Creates a timer using a fixed step value.
    /// </summary>
    public ComplexTimer(
        TCooldownType start,
        TCooldownType end,
        TCooldownType step,
        bool loop = false,
        Action onTimerEnds = null)
    {
        Start = start;
        End = end;
        Step = step;
        Loop = loop;
        OnTimerEnds = onTimerEnds;

        StepRetrieval = null;
        Timer = Start;
        IsActive = true;

        OnTimerStarts?.Invoke();
    }

    /// <summary>
    /// Creates a timer using a dynamic step retrieval function.
    /// </summary>
    public ComplexTimer(
        TCooldownType start,
        TCooldownType end,
        Func<TCooldownType> stepRetrieval,
        bool loop = false,
        Action onTimerEnds = null)
    {
        Start = start;
        End = end;
        StepRetrieval = stepRetrieval ?? throw new ArgumentNullException(nameof(stepRetrieval));
        Loop = loop;
        OnTimerEnds = onTimerEnds;

        Timer = Start;
        IsActive = true;

        OnTimerStarts?.Invoke();
    }

    /// <summary>
    /// Resets the timer to its start value.
    /// Optionally stops it.
    /// </summary>
    public void ResetTimer(bool stop = false)
    {
        Timer = Start;
        OnTimerResets?.Invoke();

        if (stop)
        {
            IsActive = false;
            OnTimerStops?.Invoke();
        }
    }

    /// <summary>
    /// Stops the timer, preventing further updates.
    /// </summary>
    public void StopTimer()
    {
        if (!IsActive)
            return;

        IsActive = false;
        OnTimerStops?.Invoke();
    }

    /// <summary>
    /// Resumes the timer if it was stopped.
    /// </summary>
    public void ResumeTimer()
    {
        if (IsActive)
            return;

        IsActive = true;
        OnTimerStarts?.Invoke();
    }

    /// <summary>
    /// Updates the timer by applying the step value.
    /// Should be called once per tick or frame.
    /// </summary>
    public void Update()
    {
        if (!IsActive)
            return;

        TCooldownType step = StepRetrieval?.Invoke() ?? Step;

        if (Type == TimerType.ADD)
        {
            Timer += step;
            if (Timer >= End)
                HandleTimerEnd();
        }
        else
        {
            Timer -= step;
            if (Timer <= End)
                HandleTimerEnd();
        }
    }

    /// <summary>
    /// Handles the logic executed when the timer reaches its end.
    /// </summary>
    private void HandleTimerEnd()
    {
        ResetTimer(!Loop);
        OnTimerEnds?.Invoke();
    }
}
