using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RCore.Service
{
    public interface IRemoteConfig
    {
	    Dictionary<string, object> GetDefaultValues();
	    void LoadRemoteValues();
    }
}