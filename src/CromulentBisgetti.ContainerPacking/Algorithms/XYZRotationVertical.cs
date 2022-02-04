using CromulentBisgetti.ContainerPacking.Entities;

using System.Linq;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    internal class XYZRotationVertical : EB_AFIT_improved
    {
        #region Protected Properties

        protected override AlgorithmType AlgorithmType => AlgorithmType.XYZRotationVertical;

        #endregion Protected Properties

        #region Protected Methods

        protected override void ExecuteIterations(Container container)
        {
            decimal bestVolume = 0;
            for (int containerOrientationVariant = 1; containerOrientationVariant <= 2; containerOrientationVariant++)
            {
                containerOrientation = containerOrientationVariant;
                switch (containerOrientationVariant)
                {
                    case 1:
                        px = container.Length;
                        py = container.Height;
                        pz = container.Width;
                        break;

                    case 2:
                        px = container.Width;
                        py = container.Height;
                        pz = container.Length;
                        break;
                }

                packedVolume = packedy = 0;
                layerThickness = remainpy = py;
                remainpz = pz;
                for (int i = 1; i <= itemsToPackCount; i++)
                {
                    itemsToPack[i].IsPacked = false;
                }

                sourceDictionaryItems = sourceItems.ToDictionary(i => i.ID, i => new Item(i));
                layerDone = false;
                PackLayer();
                if (bestVolume < packedVolume)
                {
                    bestVolume = packedVolume;
                    bestVariant = containerOrientationVariant;
                }

                if (hundredPercentPacked)
                {
                    break;
                }
            }
        }

        protected override void Report(Container container)
        {
            containerOrientation = bestVariant;
            switch (bestVariant)
            {
                case 1:
                    px = container.Length;
                    py = container.Height;
                    pz = container.Width;
                    break;

                case 2:
                    px = container.Width;
                    py = container.Height;
                    pz = container.Length;
                    break;
            }

            packingBest = true;
            packedVolume = packedy = 0;
            layerThickness = remainpy = py;
            remainpz = pz;
            for (int i = 1; i <= itemsToPackCount; i++)
            {
                itemsToPack[i].IsPacked = false;
            }

            sourceDictionaryItems = sourceItems.ToDictionary(i => i.ID, i => new Item(i));
            layerDone = false;
            PackLayer();
        }

        protected override bool SkipBoxBehind(int j) => false;

        #endregion Protected Methods

        //itemsToPack[j].ID != itemsToPack[cboxi].ID;
    }
}