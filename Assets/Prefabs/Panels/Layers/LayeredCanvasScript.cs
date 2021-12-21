using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TiltBrush.Layers
{
    public class LayeredCanvasScript : CanvasScript
    {
        BatchPool pool;
        Batch batch;

        public override void Init()
        {
            base.Init();
        }

        public override void Ignite()
        {
            pool = new BatchPool(BatchManager);
            batch = Batch.Create(pool, this.transform, BatchManager.GetBoundsOfAllStrokes());
        }

        public override void Update()
        {
            BatchManager.Update();
        }

        // when batch manager's init function is called, this canvas is set to the owner and set to start recording under the parent transform
        private void CreateBatchManager()
        {
            // Canvases might be dynamically created, so we can't rely on them all
            // being initialized at App-init time.
            Init();
            BatchManager = new BatchManager();
            if (m_BatchKeywords != null)
            {
                BatchManager.MaterialKeywords.AddRange(m_BatchKeywords);
            }
            BatchManager.Init(this);
        }

        private void UpdateBatchManager()
        {
            BatchManager.Update();
        }


    }
}

