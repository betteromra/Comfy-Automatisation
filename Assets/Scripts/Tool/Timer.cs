using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Timer
{
  protected float _timerStarted;
  public float timerStarted { get => _timerStarted; }
  protected float _timerEnded;
  public float timerEnded { get => _timerEnded; }
  [SerializeField] protected float _duration;
  public float duration { get => _duration; set => _duration = value; }
  bool _isOver;

  public Timer(float duration = 0, float timerStarted = 0)
  {
    if (timerStarted == 0) try { _timerStarted = Time.time; } catch {}
    else _timerStarted = timerStarted;

    _duration = duration;
  }

  /// <summary>
  /// Timer that loop when it's over
  /// </summary>
  /// <returns>If the timer is over or not</returns>
  public bool IsOverLoop()
  {
    bool isOverTemp = IsOver();
    if (isOverTemp) Restart();
    return isOverTemp;
  }
  /// <summary>
  /// Is over?
  /// </summary>
  /// <returns>If the timer is over or not</returns>
  public bool IsOver()
  {
    if (!_isOver) _isOver = GetTime() >= _timerStarted + _duration;
    return _isOver;
  }

  public void Restart()
  {
    _isOver = false;
    _timerStarted = GetTime();
    _timerEnded = _timerStarted + duration;
  }

  /// <summary>
  /// Perecentage of the timer done
  /// </summary>
  /// <param name="time"> time to add if needed (in millisec) <param>
  /// <returns>If the timer is over or not</returns>
  public float PercentTime(float time = 0)
  {
    if (IsOver()) return 1;
    time += GetTime();
    float deltaTime = time - _timerStarted;
    float percent = Matho.Percent(deltaTime, _duration);
    return Mathf.Clamp(percent, 0, 1);
  }
  public float TimeLeft()
  {
    if (IsOver()) return 0;
    return _timerEnded - GetTime();
  }
  public float TimePass()
  {
    if (IsOver()) return duration;
    return GetTime() - _timerStarted;
  }
  public void End()
  {
    _timerEnded = GetTime();
    _isOver = true;
  }

  public string HumanReadable()
  {
    float minutes = Mathf.Floor(TimeLeft() / 60);
    float seconds = Mathf.Floor(TimeLeft() % 60);
    float miliseconds = Mathf.Floor(TimeLeft() * 100);

    return minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + miliseconds.ToString("00");
  }

  protected virtual float GetTime()
  {
    return Time.time;
  }
}
[System.Serializable]
public class TimerReal : Timer
{
  public TimerReal(float duration = 0, float timerStarted = 0) : base(duration, timerStarted) { }

  protected override float GetTime()
  {
    return Time.realtimeSinceStartup;
  }
}
