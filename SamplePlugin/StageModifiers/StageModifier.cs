using UnityEngine;


namespace SamplePlugin.StageModifiers
{
    [System.Serializable]
    public abstract class StageModifier
    {
        public string name = "temp";
        public string activeText = "temp";
        public string reservedText = "temp";
        public int stack = 1;

        public Sprite icon;
        public Color tooltipColor;

        public int cost = 30;

        public bool negative = false;
        public bool stackable = true;

        public abstract void OnStart();

        public abstract void OnEnd();

    }

    

}
