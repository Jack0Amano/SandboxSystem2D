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
        [Header("WorldGrid")]
        public int width = 10;
        public int height = 10;
        public Vector2 cellSize = new Vector2(1, 1);

        [Header("Ground panel")]
        [Tooltip("Ground panel object which blocks can be placed")]
        [SerializeField] private GameObject ground;
        [Tooltip("Y position of blocks on the ground panel")]
        [SerializeField] private float blockYPosition = 0;

        [Header("Pewview block")]
        [Tooltip("Alpha value of the preview block. Shader rendering mode of the block should be Transparnent")]
        [SerializeField, Range(0,1)] private float previewBlockAlpha = 0.5f;
        //[SerializeField] private string alphaPropertyName = "_Alpha";

        [Header("Debug")]
        [SerializeField] private bool showGridGizmos = true;
        [SerializeField] private bool highlightSelectedCell = true;

        Camera mainCamera;

        private GameObject previewBlock;
        /// <summary>
        /// Preview block object that shows where the block will be placed
        /// </summary>
        public GameObject PreviewBlock
        {
            set
            {
                if (previewBlock != null)
                {
                    Destroy(previewBlock);
                }
                if (value != null)
                {
                    previewBlock = Instantiate(value);

                    // Change alpha value of the preview block
                    if (previewBlockAlpha != 1)
                    {
                        var renderer = previewBlock.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            var color = renderer.material.color;
                            color.a = previewBlockAlpha;
                            renderer.material.color = color;
                            //renderer.sharedMaterial.SetFloat(alphaPropertyName, previewBlockAlpha);
                        }
                    }

                    previewBlock.name = "PreviewBlock";
                    if (CursorGridIndex != new Vector2Int(int.MinValue, int.MinValue))
                    {
                        previewBlock.SetActive(true);
                        UpdateBlockPreviewPosition();
                    }
                    else
                    {
                        previewBlock.SetActive(false);
                    }
                }
            }
        }

        // Target plane for raycasting to get the world position using cursor position
        private Plane groundPlane;

        private Vector3 lastCameraPosition = Vector3.zero;
        private Vector3 lastCameraDirection = Vector3.zero;
        private Vector3 lastCursorPosition = Vector3.zero;

        readonly private List<GridBlock> allBlocks = new List<GridBlock>();

        /// <summary>
        /// All blocks gameObject that are placed on the grid
        /// </summary>
        public List<(Vector2Int grid, GameObject gameObject)> AllBlocks
        {
            get
            {
                if (allBlocks == null)
                {
                    return allBlocks.ConvertAll(x => (x.gridIndex, x.gameObject));
                }
                return new List<(Vector2Int, GameObject)>();
            }
        }

        // Column and row index what the cursor is pointing
        public Vector2Int CursorGridIndex { private set; get; } = Vector2Int.zero;

        /// <summary>
        /// Get x min position of the grids
        /// </summary>
        public float XMin
        {
            get
            {
                if (xMin != float.MinValue)
                {
                    return xMin;
                }
                var gridsCenter = ground == null ? transform.position : ground.transform.position;
                var gridWidth = width * cellSize.x;
                xMin = gridsCenter.x - gridWidth / 2;
                return xMin;
            }
        }
        private float xMin = float.MinValue;

        /// <summary>
        /// Get z min position of the grids
        /// </summary>
        public float ZMin
        {
            get
            {
                if (zMin != float.MinValue)
                {
                    return zMin;
                }
                var gridsCenter = ground == null ? transform.position : ground.transform.position;
                var gridHeight = height * cellSize.y;
                zMin = gridsCenter.z - gridHeight / 2;
                return zMin;
            }
        }
        private float zMin = float.MinValue;

        /// <summary>
        /// Get y position of the ground panel if it is set, otherwise 0
        /// </summary>
        public float YPosition
        {
            get
            {
                return ground == null ? 0 : ground.transform.position.y;
            }
        }

        private void Awake()
        {
            var planePosition = ground == null ? transform.position : ground.transform.position;
            groundPlane = new Plane(Vector3.up, planePosition);

            mainCamera = Camera.main;
        }

        /// <summary>
        /// Set block on the grid
        /// </summary>
        /// <param name="blockAsset">Block asset to set</param>
        /// <param name="gridIndex">Grid index to set the block</param>
        /// <returns>True if the block is placed successfully, False if the block cannot be placed. removedObjects are removed because duplicated</returns>
        public bool SetBlock(BlockAsset blockAsset, Vector2Int gridIndex, out List<BlockAsset> removedObjects)
        {
            if (CursorGridIndex == new Vector2Int(int.MinValue, int.MinValue))
            {
                removedObjects = new List<BlockAsset>();
                return false;
            }

            var duplicatedBlocks = allBlocks.FindAll(x => x.gridIndex == gridIndex);

            // If the block is not destroyable and there is already a block, return false
            if (duplicatedBlocks.Find(x => (!x.asset.canBeDestroyed)) != null)
            {
                removedObjects = new List<BlockAsset>();
                return false;
            }

            // Destroy all blocks that are destroyable and placed on the same grid
            foreach (var duplicated in duplicatedBlocks)
            {
                if (duplicated.asset.canBeDestroyed)
                {
                    allBlocks.Remove(duplicated);
                    Destroy(duplicated.gameObject);
                }
            }

            var position = GetWorldPositionFromGridIndex(gridIndex);
            position.y += blockYPosition;
            var block = Instantiate(blockAsset.normalObject, position, Quaternion.identity);
            block.transform.parent = transform;

            var gridBlock = new GridBlock(blockAsset, block, gridIndex);
            allBlocks.Add(gridBlock);

            removedObjects = duplicatedBlocks.ConvertAll(x => x.asset);

            return true;
        }

        /// <summary>
        /// remove block on the grid
        /// </summary>
        /// <param name="gridIndex">Grid index to remove the block</param>
        public bool RemoveBlock(Vector2Int gridIndex, out List<BlockAsset> removedObjects)
        {
            if (CursorGridIndex == new Vector2Int(int.MinValue, int.MinValue))
            {
                removedObjects = new List<BlockAsset>();
                return false;
            }

            var duplicatedBlocks = allBlocks.FindAll(x => x.gridIndex == gridIndex);

            // If the block is not destroyable and there is already a block, return false
            if (duplicatedBlocks.Find(x => (!x.asset.canBeDestroyed)) != null)
            {
                removedObjects = new List<BlockAsset>();
                return false;
            }

            // Destroy all blocks that are placed on the same grid
            foreach (var duplicated in duplicatedBlocks)
            {
                allBlocks.Remove(duplicated);
                Destroy(duplicated.gameObject);
            }
            removedObjects = duplicatedBlocks.ConvertAll(x => x.asset);

            return true;
        }

        /// <summary>
        /// Update block preview position
        /// </summary>
        public void UpdateBlockPreviewPosition()
        {
            if (previewBlock == null)
            {
                return;
            }

            if (CursorGridIndex == new Vector2Int(int.MinValue, int.MinValue))
            {
                previewBlock.SetActive(false);
                return;
            }

            if (!previewBlock.activeSelf)
            {
                previewBlock.SetActive(true);
            }

            var x = XMin + CursorGridIndex.y * cellSize.x;
            var z = ZMin + CursorGridIndex.x * cellSize.y;
            var y = YPosition + blockYPosition;

            previewBlock.transform.position = new Vector3(x + cellSize.x / 2, y, z + cellSize.y / 2);
        }

        /// <summary>
        /// Update CursorGridIndex if camera position or cursor position is changed.
        /// </summary>
        public Vector2Int UpdateCursorGridIndex(bool forceUpdate=false)
        {
            var camPosChanged = mainCamera.transform.position != lastCameraPosition;
            var camDirChanged = mainCamera.transform.forward != lastCameraDirection;
            var cursorPosChanged = Input.mousePosition != lastCursorPosition;

            if (forceUpdate || camPosChanged || camDirChanged || cursorPosChanged)
            {
                var cursorWorldPosition = GetWorldPositionFromCursor();
                CursorGridIndex = GetGridIndex(cursorWorldPosition);
                lastCameraPosition = mainCamera.transform.position;
                lastCameraDirection = mainCamera.transform.forward;
                lastCursorPosition = Input.mousePosition;
            }

            return CursorGridIndex;
        }

        /// <summary>
        /// Shoot a ray from the cursor to the ground plane and return the world position of the hit point.
        /// </summary>
        private Vector3 GetWorldPositionFromCursor()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }
        

        /// <summary>
        /// Get row and column index from world position
        /// </summary>
        private Vector2Int GetGridIndex(Vector3 world_position)
        {
            var x = world_position.x - XMin;
            var z = world_position.z - ZMin;

            var col = Mathf.FloorToInt(x / cellSize.x);
            var row = Mathf.FloorToInt(z / cellSize.y);

            if (col < 0 || col >= width || row < 0 || row >= height)
            {
                return new Vector2Int(int.MinValue, int.MinValue);
            }

            return new Vector2Int(row, col);
        }

        /// <summary>
        /// Calucate the world position of the grid index
        /// </summary>
        public Vector3 GetWorldPositionFromGridIndex(Vector2Int gridIndex)
        {
            var x = XMin + gridIndex.y * cellSize.x;
            var z = ZMin + gridIndex.x * cellSize.y;

            return new Vector3(x + cellSize.x / 2, YPosition, z + cellSize.y / 2);
        }

        private void OnDrawGizmos()
        {
            if (showGridGizmos)
            {

                // Draw grid lines
                Gizmos.color = Color.green;

                var gridWidth = width * cellSize.x;
                var gridHeight = height * cellSize.y;

                var xMin = XMin;
                var zMin = ZMin;
                var y = YPosition;

                for (int i = 0; i <= width; i++)
                {
                    var x = xMin + i * cellSize.x;
                    Gizmos.DrawLine(new Vector3(x, y, zMin), new Vector3(x, y, zMin + gridHeight));
                }
                for (int i = 0; i <= height; i++)
                {
                    var z = zMin + i * cellSize.y;
                    Gizmos.DrawLine(new Vector3(xMin, y, z), new Vector3(xMin + gridWidth, y, z));
                }
            }

            if (highlightSelectedCell)
            {
                // Highlight selected cell
                Gizmos.color = Color.red;
                var x = XMin + CursorGridIndex.y * cellSize.x;
                var z = ZMin + CursorGridIndex.x * cellSize.y;
                var y = YPosition;

                Gizmos.DrawWireCube(new Vector3(x + cellSize.x / 2, y, z + cellSize.y / 2), new Vector3(cellSize.x, 0.1f, cellSize.y));
            }

        }
    }

    /// <summary>
    /// A class that represents a block placed on the grid.
    /// </summary>
    class GridBlock
    {
        public BlockAsset asset;
        public GameObject gameObject;
        public Vector2Int gridIndex;

        public GridBlock(BlockAsset asset, GameObject gameObject, Vector2Int gridIndex)
        {
            this.asset = asset;
            this.gameObject = gameObject;
            this.gridIndex = gridIndex;
        }
    }

}
