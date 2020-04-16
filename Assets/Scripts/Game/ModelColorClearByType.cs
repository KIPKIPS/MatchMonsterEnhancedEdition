using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelColorClearByType : ModelClear {
    public ModelColor.ColorType color;
	public ModelColor.ColorType Color {
        get { return color; }
        set { color = value; }
    }

    public override void Clear() {
        base.Clear();
        modelBase.manager.ClearByType(color);
    }
	
}
