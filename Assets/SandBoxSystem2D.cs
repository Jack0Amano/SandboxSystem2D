// ================================
// Project Name: SandBoxSystem2D
// Version: 1.0
// Date: 2021/05/25
// Author: Saki Ito
// License: MIT
// ================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SandBox2D
{
    /// <summary>
    /// A system that defines a planar Grid in WorldSpace and allows objects to be placed on the plane at run-time.
    /// </summary>
    public class SandBoxSystem2D : MonoBehaviour
    {
        [Header("WorldGrid size")]
        public int width = 10;
        public int height = 10;

        [Header("Grid size")]
        public Vector2 cellSize = new Vector2(1, 1);



        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmos()
        {
            // Draw grid
            Gizmos.color = Color.green;
            for (int x = 0; x <= width; x++)
            {
                Gizmos.DrawLine(new Vector3(x * cellSize.x, 0, 0), new Vector3(x * cellSize.x, 0, height * cellSize.y));
            }
            for (int y = 0; y <= height; y++)
            {
                Gizmos.DrawLine(new Vector3(0, 0, y * cellSize.y), new Vector3(width * cellSize.x, 0, y * cellSize.y));
            }

        }
    }

}
