using CromulentBisgetti.ContainerPacking.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    public class EB_AFIT_improved : IPackingAlgorithm
    {
        #region Protected Fields

        protected readonly List<Item> itemsToPack = new List<Item>() { new Item() };
        protected int bboxi, boxi, cboxi;
        protected int bestVariant;
        protected int containerOrientation;
        protected bool hundredPercentPacked;
        protected decimal itemsToPackCount;
        protected bool layerDone;
        protected decimal layerThickness;
        protected decimal packedVolume;
        protected decimal packedy;
        protected bool packingBest;
        protected decimal px, py, pz;
        protected decimal remainpy, remainpz;
        protected Dictionary<int, Item> sourceDictionaryItems;
        protected List<Item> sourceItems;

        #endregion Protected Fields

        #region Private Fields

        private readonly List<Item> itemsPackedInOrder = new List<Item>();
        private readonly ScrapPad scrapfirst = new ScrapPad();
        private decimal bbfx, bbfy, bbfz;
        private decimal bboxx, bboxy, bboxz;
        private int bestIteration;
        private decimal bfx, bfy, bfz;
        private decimal boxx, boxy, boxz;
        private decimal cboxx, cboxy, cboxz;
        private decimal containerVolume;
        private bool evened;
        private decimal layerInLayer;
        private List<Layer> layers = new List<Layer>();
        private decimal lilz;
        private bool packing;
        private decimal prelayer;
        private decimal prepackedy;
        private decimal preremainpy;
        private ScrapPad smallestZ;
        private decimal totalItemsVolume;

        #endregion Private Fields

        #region Protected Properties

        protected virtual AlgorithmType AlgorithmType => AlgorithmType.EB_AFIT_improved;

        #endregion Protected Properties

        #region Public Methods

        public AlgorithmPackingResult Run(Container container, List<Item> items)
        {
            containerVolume = container.Volume;
            sourceItems = items.Where(i => i.Quantity > 0).OrderBy(i => i.Volume).ToList();
            for (int i = 0; i < sourceItems.Count; i++)
            {
                sourceItems[i].ID = i;
            }

            Initialize();
            ExecuteIterations(container);
            Report(container);
            AlgorithmPackingResult result = new AlgorithmPackingResult
            {
                AlgorithmID = (int)AlgorithmType,
                AlgorithmName = AlgorithmType.ToString(),
            };

            for (int i = 1; i <= itemsToPackCount; i++)
            {
                itemsToPack[i].Quantity = 1;
                if (!itemsToPack[i].IsPacked)
                {
                    result.UnpackedItems.Add(itemsToPack[i]);
                }
            }

            result.PackedItems = itemsPackedInOrder;
            result.IsCompletePack = result.UnpackedItems.Count == 0;

            return result;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void AnalyzeBoxOrientation(Action<decimal, decimal, decimal> analyzeBox, Item item)
        {
            analyzeBox(item.Dim1, item.Dim2, item.Dim3);
            if (item.Dim1 == item.Dim3 && item.Dim3 == item.Dim2)
            {
                return;
            }

            analyzeBox(item.Dim1, item.Dim3, item.Dim2);
            analyzeBox(item.Dim2, item.Dim1, item.Dim3);
            analyzeBox(item.Dim2, item.Dim3, item.Dim1);
            analyzeBox(item.Dim3, item.Dim1, item.Dim2);
            analyzeBox(item.Dim3, item.Dim2, item.Dim1);
        }

        protected virtual void ExecuteIterations(Container container)
        {
            decimal bestVolume = 0;
            for (int containerOrientationVariant = 1; containerOrientationVariant <= 6; containerOrientationVariant++)
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

                    case 3:
                        px = container.Width;
                        py = container.Length;
                        pz = container.Height;
                        break;

                    case 4:
                        px = container.Height;
                        py = container.Length;
                        pz = container.Width;
                        break;

                    case 5:
                        px = container.Length;
                        py = container.Width;
                        pz = container.Height;
                        break;

                    case 6:
                        px = container.Height;
                        py = container.Width;
                        pz = container.Length;
                        break;
                }

                layers = GetLayers();
                for (int layersIndex = 1; layersIndex < layers.Count; layersIndex++)
                {
                    packedVolume = packedy = 0;
                    packing = true;
                    layerThickness = layers[layersIndex].LayerDim;
                    remainpy = py;
                    remainpz = pz;
                    for (int i = 1; i <= itemsToPackCount; i++)
                    {
                        itemsToPack[i].IsPacked = false;
                    }

                    sourceDictionaryItems = sourceItems.ToDictionary(i => i.ID, i => new Item(i));
                    do
                    {
                        layerInLayer = 0;
                        layerDone = false;
                        PackLayer();
                        packedy += layerThickness;
                        remainpy = py - packedy;
                        if (layerInLayer != 0)
                        {
                            prepackedy = packedy;
                            preremainpy = remainpy;
                            remainpy = layerThickness - prelayer;
                            packedy = packedy - layerThickness + prelayer;
                            remainpz = lilz;
                            layerThickness = layerInLayer;
                            layerDone = false;
                            PackLayer();
                            packedy = prepackedy;
                            remainpy = preremainpy;
                            remainpz = pz;
                        }

                        FindLayer(remainpy);
                    } while (packing);

                    if (bestVolume < packedVolume)
                    {
                        bestVolume = packedVolume;
                        bestVariant = containerOrientationVariant;
                        bestIteration = layersIndex;
                    }

                    if (hundredPercentPacked)
                    {
                        break;
                    }
                }

                if (hundredPercentPacked)
                {
                    break;
                }

                if (container.Length == container.Height && container.Height == container.Width)
                {
                    containerOrientationVariant = 6;
                }
            }
        }

        protected void PackLayer()
        {
            decimal len_X, len_Z, lp_Z;
            if (layerThickness == 0)
            {
                packing = false;
                return;
            }

            scrapfirst.CumX = px;
            scrapfirst.CumZ = 0;
            while (true)
            {
                FindSmallestZ();
                if ((smallestZ.Pre == null) && (smallestZ.Post == null))
                {
                    //*** SITUATION-1: NO BOXES ON THE RIGHT AND LEFT SIDES ***
                    len_X = smallestZ.CumX;
                    lp_Z = remainpz - smallestZ.CumZ;
                    FindBox(len_X, lp_Z, lp_Z);
                    CheckFound();
                    if (layerDone)
                    {
                        break;
                    }

                    if (evened)
                    {
                        continue;
                    }

                    itemsToPack[cboxi].CoordX = 0;
                    itemsToPack[cboxi].CoordY = packedy;
                    itemsToPack[cboxi].CoordZ = smallestZ.CumZ;
                    if (cboxx == smallestZ.CumX)
                    {
                        smallestZ.CumZ += cboxz;
                    }
                    else
                    {
                        smallestZ.Post = new ScrapPad
                        {
                            Post = null,
                            Pre = smallestZ,
                            CumX = smallestZ.CumX,
                            CumZ = smallestZ.CumZ
                        };
                        smallestZ.CumX = cboxx;
                        smallestZ.CumZ += cboxz;
                    }
                }
                else if (smallestZ.Pre == null)
                {
                    //*** SITUATION-2: NO BOXES ON THE LEFT SIDE ***
                    len_X = smallestZ.CumX;
                    len_Z = smallestZ.Post.CumZ - smallestZ.CumZ;
                    lp_Z = remainpz - smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (layerDone)
                    {
                        break;
                    }

                    if (evened)
                    {
                        continue;
                    }

                    itemsToPack[cboxi].CoordY = packedy;
                    itemsToPack[cboxi].CoordZ = smallestZ.CumZ;
                    if (cboxx == smallestZ.CumX)
                    {
                        itemsToPack[cboxi].CoordX = 0;

                        if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
                        {
                            smallestZ.CumZ = smallestZ.Post.CumZ;
                            smallestZ.CumX = smallestZ.Post.CumX;
                            smallestZ.Post = smallestZ.Post.Post;
                            if (smallestZ.Post != null)
                            {
                                smallestZ.Post.Pre = smallestZ;
                            }
                        }
                        else
                        {
                            smallestZ.CumZ += cboxz;
                        }
                    }
                    else
                    {
                        itemsToPack[cboxi].CoordX = smallestZ.CumX - cboxx;
                        if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
                        {
                            smallestZ.CumX -= cboxx;
                        }
                        else
                        {
                            smallestZ.Post.Pre = new ScrapPad
                            {
                                Post = smallestZ.Post,
                                Pre = smallestZ
                            };
                            smallestZ.Post = smallestZ.Post.Pre;
                            smallestZ.Post.CumX = smallestZ.CumX;
                            smallestZ.CumX -= cboxx;
                            smallestZ.Post.CumZ = smallestZ.CumZ + cboxz;
                        }
                    }
                }
                else if (smallestZ.Post == null)
                {
                    //*** SITUATION-3: NO BOXES ON THE RIGHT SIDE ***
                    len_X = smallestZ.CumX - smallestZ.Pre.CumX;
                    len_Z = smallestZ.Pre.CumZ - smallestZ.CumZ;
                    lp_Z = remainpz - smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (layerDone)
                    {
                        break;
                    }

                    if (evened)
                    {
                        continue;
                    }

                    itemsToPack[cboxi].CoordY = packedy;
                    itemsToPack[cboxi].CoordZ = smallestZ.CumZ;
                    itemsToPack[cboxi].CoordX = smallestZ.Pre.CumX;
                    if (cboxx == smallestZ.CumX - smallestZ.Pre.CumX)
                    {
                        if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.CumX;
                            smallestZ.Pre.Post = null;
                        }
                        else
                        {
                            smallestZ.CumZ += cboxz;
                        }
                    }
                    else
                    {
                        if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
                        }
                        else
                        {
                            smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = smallestZ.Pre,
                                Post = smallestZ
                            };
                            smallestZ.Pre = smallestZ.Pre.Post;
                            smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
                            smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
                        }
                    }
                }
                else if (smallestZ.Pre.CumZ == smallestZ.Post.CumZ)
                {
                    //*** SITUATION-4: THERE ARE BOXES ON BOTH OF THE SIDES ***
                    //*** SUBSITUATION-4A: SIDES ARE EQUAL TO EACH OTHER ***
                    len_X = smallestZ.CumX - smallestZ.Pre.CumX;
                    len_Z = smallestZ.Pre.CumZ - smallestZ.CumZ;
                    lp_Z = remainpz - smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (layerDone)
                    {
                        break;
                    }

                    if (evened)
                    {
                        continue;
                    }

                    itemsToPack[cboxi].CoordY = packedy;
                    itemsToPack[cboxi].CoordZ = smallestZ.CumZ;
                    if (cboxx == smallestZ.CumX - smallestZ.Pre.CumX)
                    {
                        itemsToPack[cboxi].CoordX = smallestZ.Pre.CumX;
                        if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.Post.CumX;
                            if (smallestZ.Post.Post != null)
                            {
                                smallestZ.Pre.Post = smallestZ.Post.Post;
                                smallestZ.Post.Post.Pre = smallestZ.Pre;
                            }
                            else
                            {
                                smallestZ.Pre.Post = null;
                            }
                        }
                        else
                        {
                            smallestZ.CumZ += cboxz;
                        }
                    }
                    else if (smallestZ.Pre.CumX < px - smallestZ.CumX)
                    {
                        if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
                        {
                            smallestZ.CumX -= cboxx;
                            itemsToPack[cboxi].CoordX = smallestZ.CumX;
                        }
                        else
                        {
                            itemsToPack[cboxi].CoordX = smallestZ.Pre.CumX;
                            smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = smallestZ.Pre,
                                Post = smallestZ
                            };
                            smallestZ.Pre = smallestZ.Pre.Post;
                            smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
                            smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
                        }
                    }
                    else
                    {
                        if (smallestZ.CumZ + cboxz == smallestZ.Pre.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
                            itemsToPack[cboxi].CoordX = smallestZ.Pre.CumX;
                        }
                        else
                        {
                            itemsToPack[cboxi].CoordX = smallestZ.CumX - cboxx;
                            smallestZ.Post.Pre = new ScrapPad
                            {
                                Post = smallestZ.Post,
                                Pre = smallestZ
                            };
                            smallestZ.Post = smallestZ.Post.Pre;
                            smallestZ.Post.CumX = smallestZ.CumX;
                            smallestZ.Post.CumZ = smallestZ.CumZ + cboxz;
                            smallestZ.CumX -= cboxx;
                        }
                    }
                }
                else
                {
                    //*** SUBSITUATION-4B: SIDES ARE NOT EQUAL TO EACH OTHER ***
                    len_X = smallestZ.CumX - smallestZ.Pre.CumX;
                    len_Z = smallestZ.Pre.CumZ - smallestZ.CumZ;
                    lp_Z = remainpz - smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (layerDone)
                    {
                        break;
                    }

                    if (evened)
                    {
                        continue;
                    }

                    itemsToPack[cboxi].CoordY = packedy;
                    itemsToPack[cboxi].CoordZ = smallestZ.CumZ;
                    itemsToPack[cboxi].CoordX = smallestZ.Pre.CumX;
                    if (cboxx == (smallestZ.CumX - smallestZ.Pre.CumX))
                    {
                        if ((smallestZ.CumZ + cboxz) == smallestZ.Pre.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.CumX;
                            smallestZ.Pre.Post = smallestZ.Post;
                            smallestZ.Post.Pre = smallestZ.Pre;
                        }
                        else
                        {
                            smallestZ.CumZ += cboxz;
                        }
                    }
                    else
                    {
                        if ((smallestZ.CumZ + cboxz) == smallestZ.Pre.CumZ)
                        {
                            smallestZ.Pre.CumX = smallestZ.Pre.CumX + cboxx;
                        }
                        else if (smallestZ.CumZ + cboxz == smallestZ.Post.CumZ)
                        {
                            itemsToPack[cboxi].CoordX = smallestZ.CumX - cboxx;
                            smallestZ.CumX -= cboxx;
                        }
                        else
                        {
                            smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = smallestZ.Pre,
                                Post = smallestZ
                            };
                            smallestZ.Pre = smallestZ.Pre.Post;
                            smallestZ.Pre.CumX = smallestZ.Pre.Pre.CumX + cboxx;
                            smallestZ.Pre.CumZ = smallestZ.CumZ + cboxz;
                        }
                    }
                }

                VolumeCheck();
            }

            void FindSmallestZ()
            {
                ScrapPad scrapmemb = scrapfirst;
                smallestZ = scrapmemb;
                while (scrapmemb.Post != null)
                {
                    if (scrapmemb.Post.CumZ < smallestZ.CumZ)
                    {
                        smallestZ = scrapmemb.Post;
                    }

                    scrapmemb = scrapmemb.Post;
                }
            }

            void VolumeCheck()
            {
                sourceDictionaryItems[itemsToPack[cboxi].ID].Quantity--;
                itemsToPack[cboxi].IsPacked = true;
                itemsToPack[cboxi].PackDimX = cboxx;
                itemsToPack[cboxi].PackDimY = cboxy;
                itemsToPack[cboxi].PackDimZ = cboxz;
                packedVolume += itemsToPack[cboxi].Volume;
                PackBehindBox();
                if (packingBest)
                {
                    OutputBoxList(cboxi);
                }
                else if (packedVolume == containerVolume || packedVolume == totalItemsVolume)
                {
                    packing = false;
                    hundredPercentPacked = true;
                }
            }
        }

        protected virtual void Report(Container container)
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

                case 3:
                    px = container.Width;
                    py = container.Length;
                    pz = container.Height;
                    break;

                case 4:
                    px = container.Height;
                    py = container.Length;
                    pz = container.Width;
                    break;

                case 5:
                    px = container.Length;
                    py = container.Width;
                    pz = container.Height;
                    break;

                case 6:
                    px = container.Height;
                    py = container.Width;
                    pz = container.Length;
                    break;
            }

            layers = GetLayers();
            packingBest = true;
            packedVolume = packedy = 0;
            packing = true;
            layerThickness = layers[bestIteration].LayerDim;
            remainpy = py;
            remainpz = pz;
            for (int i = 1; i <= itemsToPackCount; i++)
            {
                itemsToPack[i].IsPacked = false;
            }

            sourceDictionaryItems = sourceItems.ToDictionary(i => i.ID, i => new Item(i));
            do
            {
                layerInLayer = 0;
                layerDone = false;
                PackLayer();
                packedy += layerThickness;
                remainpy = py - packedy;
                if (layerInLayer != 0)
                {
                    prepackedy = packedy;
                    preremainpy = remainpy;
                    remainpy = layerThickness - prelayer;
                    packedy = packedy - layerThickness + prelayer;
                    remainpz = lilz;
                    layerThickness = layerInLayer;
                    layerDone = false;
                    PackLayer();
                    packedy = prepackedy;
                    remainpy = preremainpy;
                    remainpz = pz;
                }

                FindLayer(remainpy);
            } while (packing);
        }

        protected virtual bool SkipBoxBehind(int j) => false;

        #endregion Protected Methods

        #region Private Methods

        private void CheckFound()
        {
            evened = false;
            if (boxi != 0)
            {
                cboxi = boxi;
                cboxx = boxx;
                cboxy = boxy;
                cboxz = boxz;
            }
            else
            {
                if ((bboxi > 0) && (layerInLayer != 0 || (smallestZ.Pre == null && smallestZ.Post == null)))
                {
                    if (layerInLayer == 0)
                    {
                        prelayer = layerThickness;
                        lilz = smallestZ.CumZ;
                    }

                    cboxi = bboxi;
                    cboxx = bboxx;
                    cboxy = bboxy;
                    cboxz = bboxz;
                    layerInLayer = layerInLayer + bboxy - layerThickness;
                    layerThickness = bboxy;
                }
                else
                {
                    if (smallestZ.Pre == null && smallestZ.Post == null)
                    {
                        layerDone = true;
                    }
                    else
                    {
                        evened = true;
                        if (smallestZ.Pre == null)
                        {
                            smallestZ.CumX = smallestZ.Post.CumX;
                            smallestZ.CumZ = smallestZ.Post.CumZ;
                            smallestZ.Post = smallestZ.Post.Post;
                            if (smallestZ.Post != null)
                            {
                                smallestZ.Post.Pre = smallestZ;
                            }
                        }
                        else if (smallestZ.Post == null)
                        {
                            smallestZ.Pre.Post = null;
                            smallestZ.Pre.CumX = smallestZ.CumX;
                        }
                        else
                        {
                            if (smallestZ.Pre.CumZ == smallestZ.Post.CumZ)
                            {
                                smallestZ.Pre.Post = smallestZ.Post.Post;
                                if (smallestZ.Post.Post != null)
                                {
                                    smallestZ.Post.Post.Pre = smallestZ.Pre;
                                }

                                smallestZ.Pre.CumX = smallestZ.Post.CumX;
                            }
                            else
                            {
                                smallestZ.Pre.Post = smallestZ.Post;
                                smallestZ.Post.Pre = smallestZ.Pre;
                                if (smallestZ.Pre.CumZ < smallestZ.Post.CumZ)
                                {
                                    smallestZ.Pre.CumX = smallestZ.CumX;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FindBox(decimal hmx, decimal hz, decimal hmz)
        {
            bfx = bfy = bfz = bbfx = bbfy = bbfz = 32767;
            boxi = bboxi = 0;
            int j;
            for (int i = 1; i <= itemsToPackCount; i += itemsToPack[i].Quantity)
            {
                for (j = i; j < j + itemsToPack[i].Quantity - 1; j++)
                {
                    if (!itemsToPack[j].IsPacked)
                    {
                        break;
                    }
                }

                if (itemsToPack[j].IsPacked)
                {
                    continue;
                }

                if (j > itemsToPackCount)
                {
                    return;
                }

                AnalyzeBoxOrientation(AnalyzeBox, itemsToPack[j]);
            }

            void AnalyzeBox(decimal dim1, decimal dim2, decimal dim3)
            {
                if (dim1 <= hmx && dim2 <= remainpy && dim3 <= hmz)
                {
                    if (dim2 <= layerThickness)
                    {
                        if ((layerThickness - dim2 < bfy) || ((layerThickness - dim2 == bfy) && ((hmx - dim1 < bfx) || (hmx - dim1 == bfx && Math.Abs(hz - dim3) < bfz))))
                        {
                            boxx = dim1;
                            boxy = dim2;
                            boxz = dim3;
                            bfx = hmx - dim1;
                            bfy = layerThickness - dim2;
                            bfz = Math.Abs(hz - dim3);
                            boxi = j;
                        }
                    }
                    else
                    {
                        if ((dim2 - layerThickness < bbfy) || ((dim2 - layerThickness == bbfy) && ((hmx - dim1 < bbfx) || (hmx - dim1 == bbfx && Math.Abs(hz - dim3) < bbfz))))
                        {
                            bboxx = dim1;
                            bboxy = dim2;
                            bboxz = dim3;
                            bbfx = hmx - dim1;
                            bbfy = dim2 - layerThickness;
                            bbfz = Math.Abs(hz - dim3);
                            bboxi = j;
                        }
                    }
                }
            }
        }

        private void FindLayer(decimal thickness)
        {
            decimal exDim = 0, dimen2 = 0, dimen3 = 0, eval = 1000000;
            layerThickness = 0;
            List<Item> items = sourceDictionaryItems.Values.Where(i => i.Quantity > 0).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                for (int d = 1; d <= 3; d++)
                {
                    switch (d)
                    {
                        case 1:
                            exDim = items[i].Dim1;
                            dimen2 = items[i].Dim2;
                            dimen3 = items[i].Dim3;
                            break;

                        case 2:
                            exDim = items[i].Dim2;
                            dimen2 = items[i].Dim1;
                            dimen3 = items[i].Dim3;
                            break;

                        case 3:
                            exDim = items[i].Dim3;
                            dimen2 = items[i].Dim1;
                            dimen3 = items[i].Dim2;
                            break;
                    }

                    decimal layerEval = 0;
                    if ((exDim <= thickness) && (((dimen2 <= px) && (dimen3 <= pz)) || ((dimen3 <= px) && (dimen2 <= pz))))
                    {
                        for (int j = i + 1; j < items.Count; j++)
                        {
                            layerEval += items[j].GetDimDif(exDim);
                        }

                        if (eval > layerEval)
                        {
                            eval = layerEval;
                            layerThickness = exDim;
                        }
                    }
                }
            }

            if (layerThickness == 0 || layerThickness > remainpy)
            {
                packing = false;
            }
        }

        private List<Layer> GetLayers()
        {
            List<Layer> newLayers = new List<Layer> { new Layer(0, -1) };
            decimal exDim = 0, dimen2 = 0, dimen3 = 0;
            for (int i = 0; i < sourceItems.Count; i++)
            {
                for (int d = 1; d <= 3; d++)
                {
                    switch (d)
                    {
                        case 1:
                            exDim = sourceItems[i].Dim1;
                            dimen2 = sourceItems[i].Dim2;
                            dimen3 = sourceItems[i].Dim3;
                            break;

                        case 2:
                            exDim = sourceItems[i].Dim2;
                            dimen2 = sourceItems[i].Dim1;
                            dimen3 = sourceItems[i].Dim3;
                            break;

                        case 3:
                            exDim = sourceItems[i].Dim3;
                            dimen2 = sourceItems[i].Dim1;
                            dimen3 = sourceItems[i].Dim2;
                            break;
                    }

                    if ((exDim > py) || (((dimen2 > px) || (dimen3 > pz)) && ((dimen3 > px) || (dimen2 > pz))))
                    {
                        continue;
                    }

                    if (newLayers.Any(l => l.LayerDim == exDim))
                    {
                        continue;
                    }

                    decimal layerEval = 0;
                    for (int j = i + 1; j < sourceItems.Count; j++)
                    {
                        layerEval += sourceItems[j].GetDimDif(exDim);
                    }

                    newLayers.Add(new Layer(exDim, layerEval));
                }
            }

            return newLayers.OrderBy(l => l.LayerEval).ToList();
        }

        private void Initialize()
        {
            foreach (Item itemData in sourceItems)
            {
                for (int i = 1; i <= itemData.Quantity; i++)
                {
                    itemsToPack.Add(new Item(itemData));
                }

                totalItemsVolume += itemData.TotalVolume;
                itemsToPackCount += itemData.Quantity;
            }

            itemsToPack.Add(new Item());
        }

        private void OutputBoxList(int cboxi)
        {
            decimal packCoordX = 0;
            decimal packCoordY = 0;
            decimal packCoordZ = 0;
            decimal packDimX = 0;
            decimal packDimY = 0;
            decimal packDimZ = 0;
            switch (bestVariant)
            {
                case 1:
                    packCoordX = itemsToPack[cboxi].CoordX;
                    packCoordY = itemsToPack[cboxi].CoordY;
                    packCoordZ = itemsToPack[cboxi].CoordZ;
                    packDimX = itemsToPack[cboxi].PackDimX;
                    packDimY = itemsToPack[cboxi].PackDimY;
                    packDimZ = itemsToPack[cboxi].PackDimZ;
                    break;

                case 2:
                    packCoordX = itemsToPack[cboxi].CoordZ;
                    packCoordY = itemsToPack[cboxi].CoordY;
                    packCoordZ = itemsToPack[cboxi].CoordX;
                    packDimX = itemsToPack[cboxi].PackDimZ;
                    packDimY = itemsToPack[cboxi].PackDimY;
                    packDimZ = itemsToPack[cboxi].PackDimX;
                    break;

                case 3:
                    packCoordX = itemsToPack[cboxi].CoordY;
                    packCoordY = itemsToPack[cboxi].CoordZ;
                    packCoordZ = itemsToPack[cboxi].CoordX;
                    packDimX = itemsToPack[cboxi].PackDimY;
                    packDimY = itemsToPack[cboxi].PackDimZ;
                    packDimZ = itemsToPack[cboxi].PackDimX;
                    break;

                case 4:
                    packCoordX = itemsToPack[cboxi].CoordY;
                    packCoordY = itemsToPack[cboxi].CoordX;
                    packCoordZ = itemsToPack[cboxi].CoordZ;
                    packDimX = itemsToPack[cboxi].PackDimY;
                    packDimY = itemsToPack[cboxi].PackDimX;
                    packDimZ = itemsToPack[cboxi].PackDimZ;
                    break;

                case 5:
                    packCoordX = itemsToPack[cboxi].CoordX;
                    packCoordY = itemsToPack[cboxi].CoordZ;
                    packCoordZ = itemsToPack[cboxi].CoordY;
                    packDimX = itemsToPack[cboxi].PackDimX;
                    packDimY = itemsToPack[cboxi].PackDimZ;
                    packDimZ = itemsToPack[cboxi].PackDimY;
                    break;

                case 6:
                    packCoordX = itemsToPack[cboxi].CoordZ;
                    packCoordY = itemsToPack[cboxi].CoordX;
                    packCoordZ = itemsToPack[cboxi].CoordY;
                    packDimX = itemsToPack[cboxi].PackDimZ;
                    packDimY = itemsToPack[cboxi].PackDimX;
                    packDimZ = itemsToPack[cboxi].PackDimY;
                    break;
            }

            itemsToPack[cboxi].CoordX = packCoordX;
            itemsToPack[cboxi].CoordY = packCoordY;
            itemsToPack[cboxi].CoordZ = packCoordZ;
            itemsToPack[cboxi].PackDimX = packDimX;
            itemsToPack[cboxi].PackDimY = packDimY;
            itemsToPack[cboxi].PackDimZ = packDimZ;
            itemsPackedInOrder.Add(itemsToPack[cboxi]);
        }

        private void PackBehindBox()
        {
            decimal coordX = itemsToPack[cboxi].CoordX;
            decimal coordY = itemsToPack[cboxi].CoordY + itemsToPack[cboxi].PackDimY;
            decimal coordZ = itemsToPack[cboxi].CoordZ;
            decimal remain_Y = layerThickness - cboxy;
            if (remain_Y == 0)
            {
                return;
            }

            int boxi;
            List<Item> tempList = sourceDictionaryItems.Values.Where(i => i.Quantity > 0).ToList();
            if (tempList.Count == 0)
            {
                return;
            }

            decimal minDim = tempList.Min(i => i.GetMinDim());
            if (minDim <= remain_Y && minDim <= cboxx && minDim <= cboxz)
            {
                while (true)
                {
                    if (remain_Y == 0)
                    {
                        break;
                    }

                    boxi = 0;
                    remain_Y = FindBoxBehind(cboxx, remain_Y, cboxz);
                    if (boxi == 0)
                    {
                        break;
                    }

                    sourceDictionaryItems[itemsToPack[boxi].ID].Quantity--;
                    itemsToPack[boxi].IsPacked = true;
                    itemsToPack[boxi].PackDimX = boxx;
                    itemsToPack[boxi].PackDimY = boxy;
                    itemsToPack[boxi].PackDimZ = boxz;
                    itemsToPack[boxi].CoordX = coordX;
                    itemsToPack[boxi].CoordY = coordY;
                    itemsToPack[boxi].CoordZ = coordZ;
                    coordY = itemsToPack[boxi].CoordY + itemsToPack[boxi].PackDimY;
                    packedVolume += itemsToPack[boxi].Volume;
                    if (packingBest)
                    {
                        OutputBoxList(boxi);
                    }
                }
            }

            decimal FindBoxBehind(decimal hm_X, decimal hm_Y, decimal hm_Z)
            {
                int j;
                bfx = bfy = bfz = 32767;
                for (int i = 1; i <= itemsToPackCount; i += itemsToPack[i].Quantity)
                {
                    for (j = i; j < j + itemsToPack[i].Quantity - 1; j++)
                    {
                        if (!itemsToPack[j].IsPacked)
                        {
                            break;
                        }
                    }

                    if (itemsToPack[j].IsPacked || SkipBoxBehind(j))
                    {
                        continue;
                    }

                    if (j > itemsToPackCount)
                    {
                        return bfy;
                    }

                    AnalyzeBoxOrientation(AnalyzeBox, itemsToPack[j]);
                }

                return bfy;

                void AnalyzeBox(decimal dim1, decimal dim2, decimal dim3)
                {
                    if (dim1 <= hm_X && dim2 <= hm_Y && dim3 <= hm_Z)
                    {
                        if (hm_Y - dim2 < bfy || (hm_Y - dim2 == bfy && (hm_X - dim1 < bfx || (hm_X - dim1 == bfx && hm_Z - dim3 < bfz))))
                        {
                            boxx = dim1;
                            boxy = dim2;
                            boxz = dim3;
                            bfx = hm_X - dim1;
                            bfy = hm_Y - dim2;
                            bfz = hm_Z - dim3;
                            boxi = j;
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}