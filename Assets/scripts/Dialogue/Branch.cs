using System.Collections.Generic;

[System.Serializable]
public class Branch
{
    public int priority;
    public string nextNodeId;

    public List<Condition> conditions;
}