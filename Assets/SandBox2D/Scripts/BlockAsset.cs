using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandBox2D
{
    [CreateAssetMenu(fileName = "SandBoxObjectAsset", menuName = "SandBox2D/SandBoxObjectAsset")]
    public class BlockAsset : ScriptableObject
    {
        [Header("Objects that look like when placed as stand-alone blocks")]
        [SerializeField] internal GameObject normalObject;

        [Header("Options")]
        /// <summary>
        /// Block can be destroyed
        /// </summary>
        [SerializeField] internal bool canBeDestroyed = true;
    }
}
