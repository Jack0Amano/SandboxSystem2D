using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SandBox2D
{
    public class SandBox2DDemo : MonoBehaviour
    {
        [SerializeField] private SandBoxSystem2D sandBoxSystem2D;
        [Header("Block asset to set")]
        [SerializeField] private BlockAsset blockAsset;

        [SerializeField] private TextMeshProUGUI modeLabel;

        [SerializeField] private GameObject removePreviewBlock;

        const string MODE_PLACE = "Mode: Place";
        const string MODE_REMOVE = "Mode: Remove";

        private Mode mode = Mode.Place;

        // Start is called before the first frame update
        void Start()
        {
            modeLabel.text = MODE_PLACE;
            sandBoxSystem2D.PreviewBlock = blockAsset.normalObject;
        }

        // Update is called once per frame
        private void Update()
        {
            sandBoxSystem2D.UpdateCursorGridIndex();
            sandBoxSystem2D.UpdateBlockPreviewPosition();

            // Place or remove block
            if (Input.GetMouseButtonDown(0))
            {
                if (mode == Mode.Remove)
                {
                    if (sandBoxSystem2D.RemoveBlock(sandBoxSystem2D.CursorGridIndex, out var removed))
                    {
                        Debug.Log("Block removed at " + sandBoxSystem2D.CursorGridIndex);
                    }
                }
                else
                {
                    if (sandBoxSystem2D.SetBlock(blockAsset, sandBoxSystem2D.CursorGridIndex, out var removed))
                    {
                        Debug.Log("Block placed at " + sandBoxSystem2D.CursorGridIndex);
                    }
                }
            }

            // Mode switch
            if (Input.GetKeyDown(KeyCode.E))
            {
                modeLabel.text = MODE_PLACE;
                mode = Mode.Place;
                sandBoxSystem2D.PreviewBlock = blockAsset.normalObject;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                sandBoxSystem2D.PreviewBlock = removePreviewBlock;
                modeLabel.text = MODE_REMOVE;
                mode = Mode.Remove;
            }
        }

        enum Mode
        {
            Place,
            Remove
        }
    }

}