using System.Collections.Generic;

[System.Serializable]
public class Choice
{
    public string text;
    public string nextNodeId;

    public List<Condition> conditions;
    public List<Effect> effects;
}