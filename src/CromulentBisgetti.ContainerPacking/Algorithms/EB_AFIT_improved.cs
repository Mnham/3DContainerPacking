using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    public sealed class EB_AFIT_improved
    {
        private readonly List<Item> _itemsToPack = new() { new Item() };
        private readonly List<Item> _itemsPackedInOrder = new();
        private readonly ScrapPad _scrapfirst = new();
        private int _bboxi;
        private int _boxi;
        private int _cboxi;
        private int _bestVariant;
        private bool _hundredPercentPacked;
        private decimal _itemsToPackCount;
        private bool _layerDone;
        private decimal _layerThickness;
        private decimal _packedVolume;
        private decimal _packedy;
        private bool _packingBest;
        private decimal _px;
        private decimal _py;
        private decimal _pz;
        private decimal _remainpy;
        private decimal _remainpz;
        private Dictionary<int, Item> _sourceDictionaryItems;
        private List<Item> _sourceItems;
        private decimal _bbfx;
        private decimal _bbfy;
        private decimal _bbfz;
        private decimal _bboxx;
        private decimal _bboxy;
        private decimal _bboxz;
        private int _bestIteration;
        private decimal _bfx;
        private decimal _bfy;
        private decimal _bfz;
        private decimal _boxx;
        private decimal _boxy;
        private decimal _boxz;
        private decimal _cboxx;
        private decimal _cboxy;
        private decimal _cboxz;
        private decimal _containerVolume;
        private bool _evened;
        private decimal _layerInLayer;
        private List<Layer> _layers = new();
        private decimal _lilz;
        private bool _packing;
        private decimal _prelayer;
        private decimal _prepackedy;
        private decimal _preremainpy;
        private ScrapPad _smallestZ;
        private decimal _totalItemsVolume;

        public AlgorithmPackingResult Run(Container container, List<Item> items)
        {
            _containerVolume = container.Volume;
            _sourceItems = items.Where(i => i.Quantity > 0).OrderBy(i => i.Volume).ToList();
            for (int i = 0; i < _sourceItems.Count; i++)
            {
                _sourceItems[i].ID = i;
            }

            Initialize();
            ExecuteIterations(container);
            Report(container);
            var result = new AlgorithmPackingResult();

            for (int i = 1; i <= _itemsToPackCount; i++)
            {
                _itemsToPack[i].Quantity = 1;
                if (!_itemsToPack[i].IsPacked)
                {
                    result.UnpackedItems.Add(_itemsToPack[i]);
                }
            }

            result.PackedItems = _itemsPackedInOrder;
            result.IsCompletePack = result.UnpackedItems.Count == 0;

            return result;
        }

        private void AnalyzeBoxOrientation(Action<decimal, decimal, decimal> analyzeBox, Item item)
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

        private void ExecuteIterations(Container container)
        {
            decimal bestVolume = 0;
            for (int containerOrientationVariant = 1; containerOrientationVariant <= 6; containerOrientationVariant++)
            {
                switch (containerOrientationVariant)
                {
                    case 1:
                        _px = container.Length;
                        _py = container.Height;
                        _pz = container.Width;
                        break;

                    case 2:
                        _px = container.Width;
                        _py = container.Height;
                        _pz = container.Length;
                        break;

                    case 3:
                        _px = container.Width;
                        _py = container.Length;
                        _pz = container.Height;
                        break;

                    case 4:
                        _px = container.Height;
                        _py = container.Length;
                        _pz = container.Width;
                        break;

                    case 5:
                        _px = container.Length;
                        _py = container.Width;
                        _pz = container.Height;
                        break;

                    case 6:
                        _px = container.Height;
                        _py = container.Width;
                        _pz = container.Length;
                        break;
                }

                _layers = GetLayers();
                for (int layersIndex = 1; layersIndex < _layers.Count; layersIndex++)
                {
                    _packedVolume = _packedy = 0;
                    _packing = true;
                    _layerThickness = _layers[layersIndex].LayerDim;
                    _remainpy = _py;
                    _remainpz = _pz;
                    for (int i = 1; i <= _itemsToPackCount; i++)
                    {
                        _itemsToPack[i].IsPacked = false;
                    }

                    _sourceDictionaryItems = _sourceItems.ToDictionary(i => i.ID, i => new Item(i));
                    do
                    {
                        _layerInLayer = 0;
                        _layerDone = false;
                        PackLayer();
                        _packedy += _layerThickness;
                        _remainpy = _py - _packedy;
                        if (_layerInLayer != 0)
                        {
                            _prepackedy = _packedy;
                            _preremainpy = _remainpy;
                            _remainpy = _layerThickness - _prelayer;
                            _packedy = _packedy - _layerThickness + _prelayer;
                            _remainpz = _lilz;
                            _layerThickness = _layerInLayer;
                            _layerDone = false;
                            PackLayer();
                            _packedy = _prepackedy;
                            _remainpy = _preremainpy;
                            _remainpz = _pz;
                        }

                        FindLayer(_remainpy);
                    } while (_packing);

                    if (bestVolume < _packedVolume)
                    {
                        bestVolume = _packedVolume;
                        _bestVariant = containerOrientationVariant;
                        _bestIteration = layersIndex;
                    }

                    if (_hundredPercentPacked)
                    {
                        break;
                    }
                }

                if (_hundredPercentPacked)
                {
                    break;
                }

                if (container.Length == container.Height && container.Height == container.Width)
                {
                    containerOrientationVariant = 6;
                }
            }
        }

        private void PackLayer()
        {
            decimal len_X;
            decimal len_Z;
            decimal lp_Z;
            if (_layerThickness == 0)
            {
                _packing = false;
                return;
            }

            _scrapfirst.CumX = _px;
            _scrapfirst.CumZ = 0;
            while (true)
            {
                FindSmallestZ();
                if ((_smallestZ.Pre == null) && (_smallestZ.Post == null))
                {
                    //*** SITUATION-1: NO BOXES ON THE RIGHT AND LEFT SIDES ***
                    len_X = _smallestZ.CumX;
                    lp_Z = _remainpz - _smallestZ.CumZ;
                    FindBox(len_X, lp_Z, lp_Z);
                    CheckFound();
                    if (_layerDone)
                    {
                        break;
                    }

                    if (_evened)
                    {
                        continue;
                    }

                    _itemsToPack[_cboxi].CoordX = 0;
                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    if (_cboxx == _smallestZ.CumX)
                    {
                        _smallestZ.CumZ += _cboxz;
                    }
                    else
                    {
                        _smallestZ.Post = new ScrapPad
                        {
                            Post = null,
                            Pre = _smallestZ,
                            CumX = _smallestZ.CumX,
                            CumZ = _smallestZ.CumZ
                        };
                        _smallestZ.CumX = _cboxx;
                        _smallestZ.CumZ += _cboxz;
                    }
                }
                else if (_smallestZ.Pre == null)
                {
                    //*** SITUATION-2: NO BOXES ON THE LEFT SIDE ***
                    len_X = _smallestZ.CumX;
                    len_Z = _smallestZ.Post.CumZ - _smallestZ.CumZ;
                    lp_Z = _remainpz - _smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (_layerDone)
                    {
                        break;
                    }

                    if (_evened)
                    {
                        continue;
                    }

                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    if (_cboxx == _smallestZ.CumX)
                    {
                        _itemsToPack[_cboxi].CoordX = 0;

                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                        {
                            _smallestZ.CumZ = _smallestZ.Post.CumZ;
                            _smallestZ.CumX = _smallestZ.Post.CumX;
                            _smallestZ.Post = _smallestZ.Post.Post;
                            _smallestZ.Post?.Pre = _smallestZ;
                        }
                        else
                        {
                            _smallestZ.CumZ += _cboxz;
                        }
                    }
                    else
                    {
                        _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                        {
                            _smallestZ.CumX -= _cboxx;
                        }
                        else
                        {
                            _smallestZ.Post.Pre = new ScrapPad
                            {
                                Post = _smallestZ.Post,
                                Pre = _smallestZ
                            };
                            _smallestZ.Post = _smallestZ.Post.Pre;
                            _smallestZ.Post.CumX = _smallestZ.CumX;
                            _smallestZ.CumX -= _cboxx;
                            _smallestZ.Post.CumZ = _smallestZ.CumZ + _cboxz;
                        }
                    }
                }
                else if (_smallestZ.Post == null)
                {
                    //*** SITUATION-3: NO BOXES ON THE RIGHT SIDE ***
                    len_X = _smallestZ.CumX - _smallestZ.Pre.CumX;
                    len_Z = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                    lp_Z = _remainpz - _smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (_layerDone)
                    {
                        break;
                    }

                    if (_evened)
                    {
                        continue;
                    }

                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                    if (_cboxx == _smallestZ.CumX - _smallestZ.Pre.CumX)
                    {
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.Pre.CumX = _smallestZ.CumX;
                            _smallestZ.Pre.Post = null;
                        }
                        else
                        {
                            _smallestZ.CumZ += _cboxz;
                        }
                    }
                    else
                    {
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.Pre.CumX += _cboxx;
                        }
                        else
                        {
                            _smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = _smallestZ.Pre,
                                Post = _smallestZ
                            };
                            _smallestZ.Pre = _smallestZ.Pre.Post;
                            _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                            _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                        }
                    }
                }
                else if (_smallestZ.Pre.CumZ == _smallestZ.Post.CumZ)
                {
                    //*** SITUATION-4: THERE ARE BOXES ON BOTH OF THE SIDES ***
                    //*** SUBSITUATION-4A: SIDES ARE EQUAL TO EACH OTHER ***
                    len_X = _smallestZ.CumX - _smallestZ.Pre.CumX;
                    len_Z = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                    lp_Z = _remainpz - _smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (_layerDone)
                    {
                        break;
                    }

                    if (_evened)
                    {
                        continue;
                    }

                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    if (_cboxx == _smallestZ.CumX - _smallestZ.Pre.CumX)
                    {
                        _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                        {
                            _smallestZ.Pre.CumX = _smallestZ.Post.CumX;
                            if (_smallestZ.Post.Post != null)
                            {
                                _smallestZ.Pre.Post = _smallestZ.Post.Post;
                                _smallestZ.Post.Post.Pre = _smallestZ.Pre;
                            }
                            else
                            {
                                _smallestZ.Pre.Post = null;
                            }
                        }
                        else
                        {
                            _smallestZ.CumZ += _cboxz;
                        }
                    }
                    else if (_smallestZ.Pre.CumX < _px - _smallestZ.CumX)
                    {
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.CumX -= _cboxx;
                            _itemsToPack[_cboxi].CoordX = _smallestZ.CumX;
                        }
                        else
                        {
                            _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                            _smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = _smallestZ.Pre,
                                Post = _smallestZ
                            };
                            _smallestZ.Pre = _smallestZ.Pre.Post;
                            _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                            _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                        }
                    }
                    else
                    {
                        if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.Pre.CumX += _cboxx;
                            _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                        }
                        else
                        {
                            _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                            _smallestZ.Post.Pre = new ScrapPad
                            {
                                Post = _smallestZ.Post,
                                Pre = _smallestZ
                            };
                            _smallestZ.Post = _smallestZ.Post.Pre;
                            _smallestZ.Post.CumX = _smallestZ.CumX;
                            _smallestZ.Post.CumZ = _smallestZ.CumZ + _cboxz;
                            _smallestZ.CumX -= _cboxx;
                        }
                    }
                }
                else
                {
                    //*** SUBSITUATION-4B: SIDES ARE NOT EQUAL TO EACH OTHER ***
                    len_X = _smallestZ.CumX - _smallestZ.Pre.CumX;
                    len_Z = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                    lp_Z = _remainpz - _smallestZ.CumZ;
                    FindBox(len_X, len_Z, lp_Z);
                    CheckFound();
                    if (_layerDone)
                    {
                        break;
                    }

                    if (_evened)
                    {
                        continue;
                    }

                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                    if (_cboxx == (_smallestZ.CumX - _smallestZ.Pre.CumX))
                    {
                        if ((_smallestZ.CumZ + _cboxz) == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.Pre.CumX = _smallestZ.CumX;
                            _smallestZ.Pre.Post = _smallestZ.Post;
                            _smallestZ.Post.Pre = _smallestZ.Pre;
                        }
                        else
                        {
                            _smallestZ.CumZ += _cboxz;
                        }
                    }
                    else
                    {
                        if ((_smallestZ.CumZ + _cboxz) == _smallestZ.Pre.CumZ)
                        {
                            _smallestZ.Pre.CumX += _cboxx;
                        }
                        else if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                        {
                            _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                            _smallestZ.CumX -= _cboxx;
                        }
                        else
                        {
                            _smallestZ.Pre.Post = new ScrapPad
                            {
                                Pre = _smallestZ.Pre,
                                Post = _smallestZ
                            };
                            _smallestZ.Pre = _smallestZ.Pre.Post;
                            _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                            _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                        }
                    }
                }

                VolumeCheck();
            }

            void FindSmallestZ()
            {
                ScrapPad scrapmemb = _scrapfirst;
                _smallestZ = scrapmemb;
                while (scrapmemb.Post != null)
                {
                    if (scrapmemb.Post.CumZ < _smallestZ.CumZ)
                    {
                        _smallestZ = scrapmemb.Post;
                    }

                    scrapmemb = scrapmemb.Post;
                }
            }

            void VolumeCheck()
            {
                _sourceDictionaryItems[_itemsToPack[_cboxi].ID].Quantity--;
                _itemsToPack[_cboxi].IsPacked = true;
                _itemsToPack[_cboxi].PackDimX = _cboxx;
                _itemsToPack[_cboxi].PackDimY = _cboxy;
                _itemsToPack[_cboxi].PackDimZ = _cboxz;
                _packedVolume += _itemsToPack[_cboxi].Volume;
                PackBehindBox();
                if (_packingBest)
                {
                    OutputBoxList(_cboxi);
                }
                else if (_packedVolume == _containerVolume || _packedVolume == _totalItemsVolume)
                {
                    _packing = false;
                    _hundredPercentPacked = true;
                }
            }
        }

        private void Report(Container container)
        {
            switch (_bestVariant)
            {
                case 1:
                    _px = container.Length;
                    _py = container.Height;
                    _pz = container.Width;
                    break;

                case 2:
                    _px = container.Width;
                    _py = container.Height;
                    _pz = container.Length;
                    break;

                case 3:
                    _px = container.Width;
                    _py = container.Length;
                    _pz = container.Height;
                    break;

                case 4:
                    _px = container.Height;
                    _py = container.Length;
                    _pz = container.Width;
                    break;

                case 5:
                    _px = container.Length;
                    _py = container.Width;
                    _pz = container.Height;
                    break;

                case 6:
                    _px = container.Height;
                    _py = container.Width;
                    _pz = container.Length;
                    break;
            }

            _layers = GetLayers();
            _packingBest = true;
            _packedVolume = _packedy = 0;
            _packing = true;
            _layerThickness = _layers[_bestIteration].LayerDim;
            _remainpy = _py;
            _remainpz = _pz;
            for (int i = 1; i <= _itemsToPackCount; i++)
            {
                _itemsToPack[i].IsPacked = false;
            }

            _sourceDictionaryItems = _sourceItems.ToDictionary(i => i.ID, i => new Item(i));
            do
            {
                _layerInLayer = 0;
                _layerDone = false;
                PackLayer();
                _packedy += _layerThickness;
                _remainpy = _py - _packedy;
                if (_layerInLayer != 0)
                {
                    _prepackedy = _packedy;
                    _preremainpy = _remainpy;
                    _remainpy = _layerThickness - _prelayer;
                    _packedy = _packedy - _layerThickness + _prelayer;
                    _remainpz = _lilz;
                    _layerThickness = _layerInLayer;
                    _layerDone = false;
                    PackLayer();
                    _packedy = _prepackedy;
                    _remainpy = _preremainpy;
                    _remainpz = _pz;
                }

                FindLayer(_remainpy);
            } while (_packing);
        }

        private void CheckFound()
        {
            _evened = false;
            if (_boxi != 0)
            {
                _cboxi = _boxi;
                _cboxx = _boxx;
                _cboxy = _boxy;
                _cboxz = _boxz;
            }
            else
            {
                if ((_bboxi > 0) && (_layerInLayer != 0 || (_smallestZ.Pre == null && _smallestZ.Post == null)))
                {
                    if (_layerInLayer == 0)
                    {
                        _prelayer = _layerThickness;
                        _lilz = _smallestZ.CumZ;
                    }

                    _cboxi = _bboxi;
                    _cboxx = _bboxx;
                    _cboxy = _bboxy;
                    _cboxz = _bboxz;
                    _layerInLayer = _layerInLayer + _bboxy - _layerThickness;
                    _layerThickness = _bboxy;
                }
                else
                {
                    if (_smallestZ.Pre == null && _smallestZ.Post == null)
                    {
                        _layerDone = true;
                    }
                    else
                    {
                        _evened = true;
                        if (_smallestZ.Pre == null)
                        {
                            _smallestZ.CumX = _smallestZ.Post.CumX;
                            _smallestZ.CumZ = _smallestZ.Post.CumZ;
                            _smallestZ.Post = _smallestZ.Post.Post;
                            _smallestZ.Post?.Pre = _smallestZ;
                        }
                        else if (_smallestZ.Post == null)
                        {
                            _smallestZ.Pre.Post = null;
                            _smallestZ.Pre.CumX = _smallestZ.CumX;
                        }
                        else
                        {
                            if (_smallestZ.Pre.CumZ == _smallestZ.Post.CumZ)
                            {
                                _smallestZ.Pre.Post = _smallestZ.Post.Post;
                                _smallestZ.Post.Post?.Pre = _smallestZ.Pre;

                                _smallestZ.Pre.CumX = _smallestZ.Post.CumX;
                            }
                            else
                            {
                                _smallestZ.Pre.Post = _smallestZ.Post;
                                _smallestZ.Post.Pre = _smallestZ.Pre;
                                if (_smallestZ.Pre.CumZ < _smallestZ.Post.CumZ)
                                {
                                    _smallestZ.Pre.CumX = _smallestZ.CumX;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FindBox(decimal hmx, decimal hz, decimal hmz)
        {
            _bfx = _bfy = _bfz = _bbfx = _bbfy = _bbfz = 32767;
            _boxi = _bboxi = 0;
            int j;
            for (int i = 1; i <= _itemsToPackCount; i += _itemsToPack[i].Quantity)
            {
                for (j = i; j < j + _itemsToPack[i].Quantity - 1; j++)
                {
                    if (!_itemsToPack[j].IsPacked)
                    {
                        break;
                    }
                }

                if (_itemsToPack[j].IsPacked)
                {
                    continue;
                }

                if (j > _itemsToPackCount)
                {
                    return;
                }

                AnalyzeBoxOrientation(AnalyzeBox, _itemsToPack[j]);
            }

            void AnalyzeBox(decimal dim1, decimal dim2, decimal dim3)
            {
                if (dim1 <= hmx && dim2 <= _remainpy && dim3 <= hmz)
                {
                    if (dim2 <= _layerThickness)
                    {
                        if ((_layerThickness - dim2 < _bfy) || ((_layerThickness - dim2 == _bfy) && ((hmx - dim1 < _bfx) || (hmx - dim1 == _bfx && Math.Abs(hz - dim3) < _bfz))))
                        {
                            _boxx = dim1;
                            _boxy = dim2;
                            _boxz = dim3;
                            _bfx = hmx - dim1;
                            _bfy = _layerThickness - dim2;
                            _bfz = Math.Abs(hz - dim3);
                            _boxi = j;
                        }
                    }
                    else
                    {
                        if ((dim2 - _layerThickness < _bbfy) || ((dim2 - _layerThickness == _bbfy) && ((hmx - dim1 < _bbfx) || (hmx - dim1 == _bbfx && Math.Abs(hz - dim3) < _bbfz))))
                        {
                            _bboxx = dim1;
                            _bboxy = dim2;
                            _bboxz = dim3;
                            _bbfx = hmx - dim1;
                            _bbfy = dim2 - _layerThickness;
                            _bbfz = Math.Abs(hz - dim3);
                            _bboxi = j;
                        }
                    }
                }
            }
        }

        private void FindLayer(decimal thickness)
        {
            decimal exDim = 0;
            decimal dimen2 = 0;
            decimal dimen3 = 0;
            decimal eval = 1000000;
            _layerThickness = 0;
            List<Item> items = _sourceDictionaryItems.Values.Where(i => i.Quantity > 0).ToList();
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
                    if ((exDim <= thickness) && (((dimen2 <= _px) && (dimen3 <= _pz)) || ((dimen3 <= _px) && (dimen2 <= _pz))))
                    {
                        for (int j = i + 1; j < items.Count; j++)
                        {
                            layerEval += items[j].GetDimDif(exDim);
                        }

                        if (eval > layerEval)
                        {
                            eval = layerEval;
                            _layerThickness = exDim;
                        }
                    }
                }
            }

            if (_layerThickness == 0 || _layerThickness > _remainpy)
            {
                _packing = false;
            }
        }

        private List<Layer> GetLayers()
        {
            var newLayers = new List<Layer> { new(0, -1) };
            decimal exDim = 0;
            decimal dimen2 = 0;
            decimal dimen3 = 0;
            for (int i = 0; i < _sourceItems.Count; i++)
            {
                for (int d = 1; d <= 3; d++)
                {
                    switch (d)
                    {
                        case 1:
                            exDim = _sourceItems[i].Dim1;
                            dimen2 = _sourceItems[i].Dim2;
                            dimen3 = _sourceItems[i].Dim3;
                            break;

                        case 2:
                            exDim = _sourceItems[i].Dim2;
                            dimen2 = _sourceItems[i].Dim1;
                            dimen3 = _sourceItems[i].Dim3;
                            break;

                        case 3:
                            exDim = _sourceItems[i].Dim3;
                            dimen2 = _sourceItems[i].Dim1;
                            dimen3 = _sourceItems[i].Dim2;
                            break;
                    }

                    if ((exDim > _py) || (((dimen2 > _px) || (dimen3 > _pz)) && ((dimen3 > _px) || (dimen2 > _pz))))
                    {
                        continue;
                    }

                    if (newLayers.Any(l => l.LayerDim == exDim))
                    {
                        continue;
                    }

                    decimal layerEval = 0;
                    for (int j = i + 1; j < _sourceItems.Count; j++)
                    {
                        layerEval += _sourceItems[j].GetDimDif(exDim);
                    }

                    newLayers.Add(new Layer(exDim, layerEval));
                }
            }

            return newLayers.OrderBy(l => l.LayerEval).ToList();
        }

        private void Initialize()
        {
            foreach (Item itemData in _sourceItems)
            {
                for (int i = 1; i <= itemData.Quantity; i++)
                {
                    _itemsToPack.Add(new Item(itemData));
                }

                _totalItemsVolume += itemData.TotalVolume;
                _itemsToPackCount += itemData.Quantity;
            }

            _itemsToPack.Add(new Item());
        }

        private void OutputBoxList(int cboxi)
        {
            decimal packCoordX = 0;
            decimal packCoordY = 0;
            decimal packCoordZ = 0;
            decimal packDimX = 0;
            decimal packDimY = 0;
            decimal packDimZ = 0;
            switch (_bestVariant)
            {
                case 1:
                    packCoordX = _itemsToPack[cboxi].CoordX;
                    packCoordY = _itemsToPack[cboxi].CoordY;
                    packCoordZ = _itemsToPack[cboxi].CoordZ;
                    packDimX = _itemsToPack[cboxi].PackDimX;
                    packDimY = _itemsToPack[cboxi].PackDimY;
                    packDimZ = _itemsToPack[cboxi].PackDimZ;
                    break;

                case 2:
                    packCoordX = _itemsToPack[cboxi].CoordZ;
                    packCoordY = _itemsToPack[cboxi].CoordY;
                    packCoordZ = _itemsToPack[cboxi].CoordX;
                    packDimX = _itemsToPack[cboxi].PackDimZ;
                    packDimY = _itemsToPack[cboxi].PackDimY;
                    packDimZ = _itemsToPack[cboxi].PackDimX;
                    break;

                case 3:
                    packCoordX = _itemsToPack[cboxi].CoordY;
                    packCoordY = _itemsToPack[cboxi].CoordZ;
                    packCoordZ = _itemsToPack[cboxi].CoordX;
                    packDimX = _itemsToPack[cboxi].PackDimY;
                    packDimY = _itemsToPack[cboxi].PackDimZ;
                    packDimZ = _itemsToPack[cboxi].PackDimX;
                    break;

                case 4:
                    packCoordX = _itemsToPack[cboxi].CoordY;
                    packCoordY = _itemsToPack[cboxi].CoordX;
                    packCoordZ = _itemsToPack[cboxi].CoordZ;
                    packDimX = _itemsToPack[cboxi].PackDimY;
                    packDimY = _itemsToPack[cboxi].PackDimX;
                    packDimZ = _itemsToPack[cboxi].PackDimZ;
                    break;

                case 5:
                    packCoordX = _itemsToPack[cboxi].CoordX;
                    packCoordY = _itemsToPack[cboxi].CoordZ;
                    packCoordZ = _itemsToPack[cboxi].CoordY;
                    packDimX = _itemsToPack[cboxi].PackDimX;
                    packDimY = _itemsToPack[cboxi].PackDimZ;
                    packDimZ = _itemsToPack[cboxi].PackDimY;
                    break;

                case 6:
                    packCoordX = _itemsToPack[cboxi].CoordZ;
                    packCoordY = _itemsToPack[cboxi].CoordX;
                    packCoordZ = _itemsToPack[cboxi].CoordY;
                    packDimX = _itemsToPack[cboxi].PackDimZ;
                    packDimY = _itemsToPack[cboxi].PackDimX;
                    packDimZ = _itemsToPack[cboxi].PackDimY;
                    break;
            }

            _itemsToPack[cboxi].CoordX = packCoordX;
            _itemsToPack[cboxi].CoordY = packCoordY;
            _itemsToPack[cboxi].CoordZ = packCoordZ;
            _itemsToPack[cboxi].PackDimX = packDimX;
            _itemsToPack[cboxi].PackDimY = packDimY;
            _itemsToPack[cboxi].PackDimZ = packDimZ;
            _itemsPackedInOrder.Add(_itemsToPack[cboxi]);
        }

        private void PackBehindBox()
        {
            decimal coordX = _itemsToPack[_cboxi].CoordX;
            decimal coordY = _itemsToPack[_cboxi].CoordY + _itemsToPack[_cboxi].PackDimY;
            decimal coordZ = _itemsToPack[_cboxi].CoordZ;
            decimal remain_Y = _layerThickness - _cboxy;
            if (remain_Y == 0)
            {
                return;
            }

            int boxi;
            List<Item> tempList = _sourceDictionaryItems.Values.Where(i => i.Quantity > 0).ToList();
            if (tempList.Count == 0)
            {
                return;
            }

            decimal minDim = tempList.Min(i => i.GetMinDim());
            if (minDim <= remain_Y && minDim <= _cboxx && minDim <= _cboxz)
            {
                while (remain_Y != 0)
                {
                    boxi = 0;
                    remain_Y = FindBoxBehind(_cboxx, remain_Y, _cboxz);
                    if (boxi == 0)
                    {
                        break;
                    }

                    _sourceDictionaryItems[_itemsToPack[boxi].ID].Quantity--;
                    _itemsToPack[boxi].IsPacked = true;
                    _itemsToPack[boxi].PackDimX = _boxx;
                    _itemsToPack[boxi].PackDimY = _boxy;
                    _itemsToPack[boxi].PackDimZ = _boxz;
                    _itemsToPack[boxi].CoordX = coordX;
                    _itemsToPack[boxi].CoordY = coordY;
                    _itemsToPack[boxi].CoordZ = coordZ;
                    coordY = _itemsToPack[boxi].CoordY + _itemsToPack[boxi].PackDimY;
                    _packedVolume += _itemsToPack[boxi].Volume;
                    if (_packingBest)
                    {
                        OutputBoxList(boxi);
                    }
                }
            }

            decimal FindBoxBehind(decimal hm_X, decimal hm_Y, decimal hm_Z)
            {
                int j;
                _bfx = _bfy = _bfz = 32767;
                for (int i = 1; i <= _itemsToPackCount; i += _itemsToPack[i].Quantity)
                {
                    for (j = i; j < j + _itemsToPack[i].Quantity - 1; j++)
                    {
                        if (!_itemsToPack[j].IsPacked)
                        {
                            break;
                        }
                    }

                    if (_itemsToPack[j].IsPacked)
                    {
                        continue;
                    }

                    if (j > _itemsToPackCount)
                    {
                        return _bfy;
                    }

                    AnalyzeBoxOrientation(AnalyzeBox, _itemsToPack[j]);
                }

                return _bfy;

                void AnalyzeBox(decimal dim1, decimal dim2, decimal dim3)
                {
                    if (dim1 <= hm_X && dim2 <= hm_Y && dim3 <= hm_Z)
                    {
                        if (hm_Y - dim2 < _bfy || (hm_Y - dim2 == _bfy && (hm_X - dim1 < _bfx || (hm_X - dim1 == _bfx && hm_Z - dim3 < _bfz))))
                        {
                            _boxx = dim1;
                            _boxy = dim2;
                            _boxz = dim3;
                            _bfx = hm_X - dim1;
                            _bfy = hm_Y - dim2;
                            _bfz = hm_Z - dim3;
                            boxi = j;
                        }
                    }
                }
            }
        }
    }
}
