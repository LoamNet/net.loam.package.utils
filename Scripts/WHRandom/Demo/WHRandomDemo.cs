using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace Loam.Internal.Demo
{
    public class WHRandomDemo : MonoBehaviour
    {
        // Inspector Variables
        [Header("Debug Config")]
        [SerializeField] private UnityEngine.UI.Text displayText = null;
        [Range(0, 5000)] [SerializeField] private int iterations = 1000;
        [Range(1, 50)] [SerializeField] private int bucketCount = 10;

        [Header("WHRandom Config")]
        [SerializeField] private int seed = 123;

        // Internal Variables
        private WHRandom random;
        private int[] buckets;
        private int entries;
        private double bucketSize;



#if UNITY_EDITOR
        // Refresh if we're changing variables
        private void OnValidate()
        {
            Initialize();
        }
#endif

        // Allow for re-initializing whenever
        public void Initialize()
        {
            random = new WHRandom(seed);
            buckets = new int[bucketCount];
            entries = 0;
            bucketSize = 1.0d / bucketCount;
        }

        // Configure initially
        private void Start()
        {
            Initialize();
        }

        // Update is called once per frame
        private void Update()
        {
            if (entries < iterations)
            {
                // Generate next iteration
                double generated = random.Next();
                int index = (int)(generated / bucketSize);

                ++buckets[index];
                ++entries;

                // Redraw debug text
                string assembled = "";
                assembled += $"Internal seed: {random.Seed}, entries: {entries}";
                for (int bucket = 0; bucket < bucketCount; ++bucket)
                {
                    string low = string.Format("{0:0.00}", bucket * bucketSize);
                    string high = string.Format("{0:0.00}", (bucket + 1) * bucketSize);
                    string entries = string.Format("{0:000}", buckets[bucket]);

                    assembled += $"\n{low}-{high}, [{entries}]: ";
                    assembled += new string('|', buckets[bucket]);
                }

                displayText.text = assembled;
            }
        }
    }
}