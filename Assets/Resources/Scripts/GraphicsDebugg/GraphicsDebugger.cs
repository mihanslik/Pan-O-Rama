using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsDebugger : MonoBehaviour
{
    /// Blit with this material has to be active in renderer settings
    public Material ChannelSplit;

    public void SetChannelSplit(int option)
    {
        ChannelSplit.SetFloat("_Option", option);
    }


}
