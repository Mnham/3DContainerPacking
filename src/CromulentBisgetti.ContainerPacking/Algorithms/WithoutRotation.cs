using CromulentBisgetti.ContainerPacking.Entities;

using System;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    internal class WithoutRotation : EB_AFIT_improved
    {
        #region Protected Properties

        protected override AlgorithmType AlgorithmType => AlgorithmType.WithoutRotation;

        #endregion Protected Properties

        #region Protected Methods

        protected override void AnalyzeBoxOrientation(Action<decimal, decimal, decimal> analyzeBox, Item item)
        {
            switch (containerOrientation)
            {
                case 1:
                    analyzeBox(item.Dim1, item.Dim3, item.Dim2);
                    break;

                case 2:
                    analyzeBox(item.Dim2, item.Dim3, item.Dim1);
                    break;

                case 3:
                    analyzeBox(item.Dim2, item.Dim1, item.Dim3);
                    break;

                case 4:
                    analyzeBox(item.Dim3, item.Dim1, item.Dim2);
                    break;

                case 5:
                    analyzeBox(item.Dim1, item.Dim2, item.Dim3);
                    break;

                case 6:
                    analyzeBox(item.Dim3, item.Dim2, item.Dim1);
                    break;
            }
        }

        #endregion Protected Methods
    }
}