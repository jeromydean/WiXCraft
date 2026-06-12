using System;
using WixToolset.Dtf.WindowsInstaller;

namespace WiXCraft
{
  /// <summary>
  /// Tracks MSI progress messages and converts them to usable progress.
  /// </summary>
  public sealed class InstallProgressCounter
  {
    private int total;
    private int completed;
    private int step;
    private bool moveForward;
    private bool enableActionData;
    private int progressPhase;
    private readonly double scriptPhaseWeight;

    public InstallProgressCounter()
      : this(0.3)
    {
    }

    public InstallProgressCounter(double scriptPhaseWeight)
    {
      if (scriptPhaseWeight < 0 || scriptPhaseWeight > 1)
      {
        throw new ArgumentOutOfRangeException(nameof(scriptPhaseWeight));
      }

      this.scriptPhaseWeight = scriptPhaseWeight;
    }

    public double Progress { get; private set; }

    public void ProcessMessage(InstallMessage messageType, Record messageRecord)
    {
      switch (messageType)
      {
        case InstallMessage.ActionStart:
          if (enableActionData)
          {
            enableActionData = false;
          }

          break;

        case InstallMessage.ActionData:
          if (enableActionData)
          {
            if (moveForward)
            {
              completed += step;
            }
            else
            {
              completed -= step;
            }

            UpdateProgress();
          }

          break;

        case InstallMessage.Progress:
          ProcessProgressMessage(messageRecord);
          break;
      }
    }

    private void ProcessProgressMessage(Record progressRecord)
    {
      if (progressRecord == null || progressRecord.FieldCount == 0)
      {
        return;
      }

      int fieldCount = progressRecord.FieldCount;
      int progressType = progressRecord.GetInteger(1);

      switch (progressType)
      {
        case 0:
          if (fieldCount < 4)
          {
            return;
          }

          progressPhase++;
          total = progressRecord.GetInteger(2);

          if (progressPhase == 1)
          {
            total += 50;
          }

          moveForward = progressRecord.GetInteger(3) == 0;
          completed = moveForward ? 0 : total;
          enableActionData = false;
          UpdateProgress();
          break;

        case 1:
          if (fieldCount < 3)
          {
            return;
          }

          if (progressRecord.GetInteger(3) == 0)
          {
            enableActionData = false;
          }
          else
          {
            enableActionData = true;
            step = progressRecord.GetInteger(2);
          }

          break;

        case 2:
          if (fieldCount < 2 || total == 0 || progressPhase == 0)
          {
            return;
          }

          if (moveForward)
          {
            completed += progressRecord.GetInteger(2);
          }
          else
          {
            completed -= progressRecord.GetInteger(2);
          }

          UpdateProgress();
          break;

        case 3:
          total += progressRecord.GetInteger(2);
          break;
      }
    }

    private void UpdateProgress()
    {
      if (progressPhase < 1 || total == 0)
      {
        Progress = 0;
      }
      else if (progressPhase == 1)
      {
        Progress = scriptPhaseWeight * Math.Min(completed, total) / total;
      }
      else if (progressPhase == 2)
      {
        Progress = scriptPhaseWeight +
          (1 - scriptPhaseWeight) * Math.Min(completed, total) / total;
      }
      else
      {
        Progress = 1;
      }
    }
  }
}