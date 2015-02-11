public class HudInventory
{
    public HudInventory()
    {
        //indexed by enum WearPlace
        wearPlaceStart = new PointRef[5];
        {
            //new Point(282,85), //LeftHand,
            wearPlaceStart[0] = PointRef.Create(40, 110); //RightHand,
            wearPlaceStart[1] = PointRef.Create(80, 110); //MainArmor,
            wearPlaceStart[2] = PointRef.Create(200, 110); //Boots,
            wearPlaceStart[3] = PointRef.Create(120, 110); //Helmet,
            wearPlaceStart[4] = PointRef.Create(160, 110); //Gauntlet,
        }

        //indexed by enum WearPlace
        wearPlaceCells = new PointRef[5];
        {
            //new Point(2,4), //LeftHand,
            wearPlaceCells[0] = PointRef.Create(1, 1); //RightHand,
            wearPlaceCells[1] = PointRef.Create(1, 1); //MainArmor,
            wearPlaceCells[2] = PointRef.Create(1, 1); //Boots,
            wearPlaceCells[3] = PointRef.Create(1, 1); //Helmet,
            wearPlaceCells[4] = PointRef.Create(1, 1); //Gauntlet,
        }
        CellCountInPageX = 12;
        CellCountInPageY = 7;
        CellCountTotalX = 12;
        CellCountTotalY = 7 * 6;
        CellDrawSize = 40;
    }

    internal Game game;
    internal GameDataItemsClient dataItems;
    internal InventoryUtilClient inventoryUtil;
    internal IInventoryController controller;

    internal int CellDrawSize;

    //Position of crafting interface
    int CraftingInterfaceStartX() { return InventoryStartX() + 580 ; }
    int CraftingInterfaceStartY() { return InventoryStartY() + 100; }

    public int InventoryStartX() { return game.Width() / 2 - 560 / 2; }
    public int InventoryStartY() { return game.Height() / 2 - 600 / 2; }
    public int CellsStartX() { return 33 + InventoryStartX(); }
    public int CellsStartY() { return 180 + InventoryStartY(); }
    int MaterialSelectorStartX() { return game.platform.FloatToInt(MaterialSelectorBackgroundStartX() + 17 * game.Scale()); }
    int MaterialSelectorStartY() { return game.platform.FloatToInt(MaterialSelectorBackgroundStartY() + 17 * game.Scale()); }
    int MaterialSelectorBackgroundStartX() { return game.platform.FloatToInt(game.Width() / 2 - (512 / 2) * game.Scale()); }
    int MaterialSelectorBackgroundStartY() { return game.platform.FloatToInt(game.Height() - 90 * game.Scale()); }
    int CellCountInPageX;
    int CellCountInPageY;
    int CellCountTotalX;
    int CellCountTotalY;

    public int ActiveMaterialCellSize() { return game.platform.FloatToInt(48 * game.Scale()); }

    public void OnKeyPress(int keyChar)
    {
        if (keyChar == 49) { game.ActiveMaterial = 0; }
        if (keyChar == 50) { game.ActiveMaterial = 1; }
        if (keyChar == 51) { game.ActiveMaterial = 2; }
        if (keyChar == 52) { game.ActiveMaterial = 3; }
        if (keyChar == 53) { game.ActiveMaterial = 4; }
        if (keyChar == 54) { game.ActiveMaterial = 5; }
        if (keyChar == 55) { game.ActiveMaterial = 6; }
        if (keyChar == 56) { game.ActiveMaterial = 7; }
        if (keyChar == 57) { game.ActiveMaterial = 8; }
        if (keyChar == 48) { game.ActiveMaterial = 9; }
    }
    int ScrollButtonSize() { return CellDrawSize; }

    int ScrollUpButtonX() { return CellsStartX() + CellCountInPageX * CellDrawSize; }
    int ScrollUpButtonY() { return CellsStartY(); }

    int ScrollDownButtonX() { return CellsStartX() + CellCountInPageX * CellDrawSize; }
    int ScrollDownButtonY() { return CellsStartY() + (CellCountInPageY - 1) * CellDrawSize; }

    public bool Mouse_ButtonDown(MouseEventArgs args)
    {
        PointRef scaledMouse = PointRef.Create(args.GetX(), args.GetY());

        //material selector
        if (SelectedMaterialSelectorSlot(scaledMouse) != null)
        {
            //int oldActiveMaterial = ActiveMaterial.ActiveMaterial;
            game.ActiveMaterial = SelectedMaterialSelectorSlot(scaledMouse).value;
            //if (oldActiveMaterial == ActiveMaterial.ActiveMaterial)
            {
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                p.Type = Packet_InventoryPositionTypeEnum.MaterialSelector;
                p.MaterialId = game.ActiveMaterial;
                controller.InventoryClick(p);
            }
            return true;
        }

        if (game.guistate != GuiState.Inventory)
        {
            return false;
        }

        //Check if the click is in the crafting inventory interface
        PointRef cellinCraft = SelectedCraftCell(scaledMouse);

        

        if(cellinCraft != null)
        {
            if (args.GetButton() == MouseButtonEnum.Right)
            {
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                p.Type = Packet_InventoryPositionTypeEnum.Crafting;
                p.AreaX = cellinCraft.X;
                p.AreaY = cellinCraft.Y;
                controller.InventoryRightClick(p);
                return true;
            }

            Packet_InventoryPosition p1 = new Packet_InventoryPosition();
            p1.Type = Packet_InventoryPositionTypeEnum.Crafting;
            p1.AreaX = cellinCraft.X;
            p1.AreaY = cellinCraft.Y;
            controller.InventoryClick(p1);
            return true;
        }

       

        //main inventory
        PointRef cellInPage = SelectedCell(scaledMouse);
#region grab from inventory
        if (cellInPage != null)
        {
            if (args.GetButton() == MouseButtonEnum.Right)
            {
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                p.AreaX = cellInPage.X;
                p.AreaY = cellInPage.Y;
                controller.InventoryRightClick(p);
                return true;
            }

            if (args.GetButton() == MouseButtonEnum.Left)
            {
                Packet_InventoryPosition p = new Packet_InventoryPosition();
                p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                p.AreaX = cellInPage.X;
                p.AreaY = cellInPage.Y + ScrollLine;
                controller.InventoryClick(p);
                return true;
            }
            else
            {
                {
                    Packet_InventoryPosition p = new Packet_InventoryPosition();
                    p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                    p.AreaX = cellInPage.X;
                    p.AreaY = cellInPage.Y + ScrollLine;
                    controller.InventoryClick(p);
                }
                {
                    Packet_InventoryPosition p = new Packet_InventoryPosition();
                    p.Type = Packet_InventoryPositionTypeEnum.WearPlace;
                    p.WearPlace = WearPlace_.RightHand;
                    p.ActiveMaterial = game.ActiveMaterial;
                    controller.InventoryClick(p);
                }
                {
                    Packet_InventoryPosition p = new Packet_InventoryPosition();
                    p.Type = Packet_InventoryPositionTypeEnum.MainArea;
                    p.AreaX = cellInPage.X;
                    p.AreaY = cellInPage.Y + ScrollLine;
                    controller.InventoryClick(p);
                }
            }
            if (game.guistate == GuiState.Inventory)
            {
                return true;
            }
        }
#endregion

        // //drop items on ground
        //if (scaledMouse.X < CellsStartX() && scaledMouse.Y < MaterialSelectorStartY())
        //{
        //    int posx = game.SelectedBlockPositionX;
        //    int posy = game.SelectedBlockPositionY;
        //    int posz = game.SelectedBlockPositionZ;
        //    Packet_InventoryPosition p = new Packet_InventoryPosition();
        //    {
        //        p.Type = Packet_InventoryPositionTypeEnum.Ground;
        //        p.GroundPositionX = posx;
        //        p.GroundPositionY = posy;
        //        p.GroundPositionZ = posz;
        //    }
        //    controller.InventoryClick(p);
        //}
        if (SelectedWearPlace(scaledMouse) != null)
        {
            Packet_InventoryPosition p = new Packet_InventoryPosition();
            p.Type = Packet_InventoryPositionTypeEnum.WearPlace;
            p.WearPlace = (SelectedWearPlace(scaledMouse).value);
            p.ActiveMaterial = game.ActiveMaterial;
            controller.InventoryClick(p);
            return true;
        }
        if (scaledMouse.X >= ScrollUpButtonX() && scaledMouse.X < ScrollUpButtonX() + ScrollButtonSize()
            && scaledMouse.Y >= ScrollUpButtonY() && scaledMouse.Y < ScrollUpButtonY() + ScrollButtonSize())
        {
            ScrollUp();
            ScrollingUpTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
            return true;
        }
        if (scaledMouse.X >= ScrollDownButtonX() && scaledMouse.X < ScrollDownButtonX() + ScrollButtonSize()
            && scaledMouse.Y >= ScrollDownButtonY() && scaledMouse.Y < ScrollDownButtonY() + ScrollButtonSize())
        {
            ScrollDown();
            ScrollingDownTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
            return true;
        }
        //Disabled this method, was kinda frustating to click some pixel on the side and get a closed inventory
      //  game.GuiStateBackToGame();
        return false;
    }

    public bool IsMouseOverCells()
    {
        return SelectedCellOrScrollbar(game.mouseCurrentX, game.mouseCurrentY);
    }

    public void ScrollUp()
    {
        ScrollLine--;
        if (ScrollLine < 0) { ScrollLine = 0; }
    }

    public void ScrollDown()
    {
        ScrollLine++;
        int max = CellCountTotalY - CellCountInPageY;
        if (ScrollLine >= max) { ScrollLine = max; }
    }

    int ScrollingUpTimeMilliseconds;
    int ScrollingDownTimeMilliseconds;

    public void Mouse_ButtonUp(MouseEventArgs args)
    {
        ScrollingUpTimeMilliseconds = 0;
        ScrollingDownTimeMilliseconds = 0;
    }

    IntRef SelectedMaterialSelectorSlot(PointRef scaledMouse)
    {
        if (scaledMouse.X >= MaterialSelectorStartX() && scaledMouse.Y >= MaterialSelectorStartY()
            && scaledMouse.X < MaterialSelectorStartX() + 10 * ActiveMaterialCellSize()
            && scaledMouse.Y < MaterialSelectorStartY() + 10 * ActiveMaterialCellSize())
        {
            return IntRef.Create((scaledMouse.X - MaterialSelectorStartX()) / ActiveMaterialCellSize());
        }
        return null;
    }

    PointRef SelectedCell(PointRef scaledMouse)
    {
        if (scaledMouse.X < CellsStartX() || scaledMouse.Y < CellsStartY()
            || scaledMouse.X > CellsStartX() + CellCountInPageX * CellDrawSize
            || scaledMouse.Y > CellsStartY() + CellCountInPageY * CellDrawSize)
        {
            return null;
        }
        PointRef cell = PointRef.Create((scaledMouse.X - CellsStartX()) / CellDrawSize,
            (scaledMouse.Y - CellsStartY()) / CellDrawSize);
        return cell;
    }

    /// <summary>
    /// Mathew Lemonde
    /// Code to know if the click is in the crafting interface
    /// </summary>
    /// <param name="scaledMouse">Mouse click position</param>
    /// <returns>Selected cell or null</returns>
    PointRef SelectedCraftCell(PointRef scaledMouse)
    {
        if (scaledMouse.X < CraftingInterfaceStartX() || scaledMouse.Y < CraftingInterfaceStartY()
           || scaledMouse.X > CraftingInterfaceStartX() + 5 * CellDrawSize
           || scaledMouse.Y > CraftingInterfaceStartY() + 4 * CellDrawSize)
        {
            return null;
        }
        PointRef cell = PointRef.Create((scaledMouse.X - CraftingInterfaceStartX()) / CellDrawSize,
            (scaledMouse.Y - CraftingInterfaceStartY()) / CellDrawSize);

        //The cells returned by previous code is from a 5x5 array-like
        //The image for the crafting interface is a 3x3 array and a separate block for craft result
        //The following if remove cells from that 5x5 array who are not in the image

        if (cell.X == 3)
            return null;

        if (cell.Y == 0)
            return null;

        if (cell.X == 4)
            if (cell.Y == 1)
                return null;
            else if (cell.Y == 3)
                return null;


        return cell;
    }

    bool SelectedCellOrScrollbar(int scaledMouseX, int scaledMouseY)
    {
        if (scaledMouseX < CellsStartX() || scaledMouseY < CellsStartY()
            || scaledMouseX > CellsStartX() + (CellCountInPageX + 1) * CellDrawSize
            || scaledMouseY > CellsStartY() + CellCountInPageY * CellDrawSize)
        {
            return false;
        }
        return true;
    }

    internal int ScrollLine;

    public void Draw()
    {

        if (ScrollingUpTimeMilliseconds != 0 && (game.platform.TimeMillisecondsFromStart() - ScrollingUpTimeMilliseconds) > 250)
        {
            ScrollingUpTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
            ScrollUp();
        }
        if (ScrollingDownTimeMilliseconds != 0 && (game.platform.TimeMillisecondsFromStart() - ScrollingDownTimeMilliseconds) > 250)
        {
            ScrollingDownTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
            ScrollDown();
        }

        PointRef scaledMouse = PointRef.Create(game.mouseCurrentX, game.mouseCurrentY);

        game.Draw2dBitmapFile("inventory.png", InventoryStartX() + 5, InventoryStartY() + 6, 1024, 1024);
        game.Draw2dBitmapFile("crafting.png", CraftingInterfaceStartX(), CraftingInterfaceStartY(), 240, 240);

        //the3d.Draw2dTexture(terrain, 50, 50, 50, 50, 0);
        //the3d.Draw2dBitmapFile("inventory_weapon_shovel.png", 100, 100, 60 * 2, 60 * 4);
        //the3d.Draw2dBitmapFile("inventory_gauntlet_gloves.png", 200, 200, 60 * 2, 60 * 2);
        //main inventory
        for (int i = 0; i < game.d_Inventory.ItemsCount; i++)
        {
            Packet_PositionItem k = game.d_Inventory.Items[i];
            if (k == null)
            {
                continue;
            }
            int screeny = k.Y - ScrollLine;
            if (screeny >= 0 && screeny < CellCountInPageY)
            {
                DrawItem(CellsStartX() + k.X * CellDrawSize + 7, CellsStartY() + screeny * CellDrawSize + 7, k.Value_, 0, 0);
            }
        }

        //Crafting inventory Draw
        for (int i = 0; i < game.d_Inventory.CraftItemsCount; i++ )
        {
            Packet_PositionItem k = game.d_Inventory.CraftItems[i];
            if (k == null)
            {
                continue;
            }
            
                DrawItem(CraftingInterfaceStartX() + k.X * CellDrawSize, CraftingInterfaceStartY() + k.Y* CellDrawSize, k.Value_, 0, 0);
        }

            //draw area selection
            if (game.d_Inventory.DragDropItem != null)
            {
                PointRef selectedInPage = SelectedCell(scaledMouse);
                if (selectedInPage != null)
                {
                    int x = (selectedInPage.X) * CellDrawSize + CellsStartX();
                    int y = (selectedInPage.Y) * CellDrawSize + CellsStartY();
                    int sizex = dataItems.ItemSizeX(game.d_Inventory.DragDropItem);
                    int sizey = dataItems.ItemSizeY(game.d_Inventory.DragDropItem);
                    if (selectedInPage.X + sizex <= CellCountInPageX
                        && selectedInPage.Y + sizey <= CellCountInPageY)
                    {
                        int c;
                        IntRef itemsAtAreaCount = new IntRef();
                        PointRef[] itemsAtArea = inventoryUtil.ItemsAtArea(selectedInPage.X, selectedInPage.Y + ScrollLine, sizex, sizey, itemsAtAreaCount);
                        if (itemsAtArea == null || itemsAtAreaCount.value > 1)
                        {
                            c = Game.ColorFromArgb(100, 255, 0, 0); // red
                        }
                        else //0 or 1
                        {
                            c = Game.ColorFromArgb(100, 0, 255, 0); // green
                        }
                        game.Draw2dTexture(game.WhiteTexture(), x, y,
                            CellDrawSize * sizex, CellDrawSize * sizey,
                            null, 0, c, false);
                    }
                }
                IntRef selectedWear = SelectedWearPlace(scaledMouse);
                if (selectedWear != null)
                {
                    PointRef p = PointRef.Create(wearPlaceStart[selectedWear.value].X + InventoryStartX(), wearPlaceStart[selectedWear.value].Y + InventoryStartY());
                    PointRef size = wearPlaceCells[selectedWear.value];

                    int c;
                    Packet_Item itemsAtArea = inventoryUtil.ItemAtWearPlace(selectedWear.value, game.ActiveMaterial);
                    if (!dataItems.CanWear(selectedWear.value, game.d_Inventory.DragDropItem))
                    {
                        c = Game.ColorFromArgb(100, 255, 0, 0); // red
                    }
                    else //0 or 1
                    {
                        c = Game.ColorFromArgb(100, 0, 255, 0); // green
                    }
                    game.Draw2dTexture(game.WhiteTexture(), p.X, p.Y,
                        CellDrawSize * size.X, CellDrawSize * size.Y,
                        null, 0, c, false);
                }
            }

        //material selector
        DrawMaterialSelector();

        //wear
        //DrawItem(Offset(wearPlaceStart[(int)WearPlace.LeftHand], InventoryStart), inventory.LeftHand[ActiveMaterial.ActiveMaterial], null);
        DrawItem(wearPlaceStart[WearPlace_.RightHand].X + InventoryStartX(), wearPlaceStart[WearPlace_.RightHand].Y + InventoryStartY(), game.d_Inventory.RightHand[game.ActiveMaterial], 0, 0);
        DrawItem(wearPlaceStart[WearPlace_.MainArmor].X + InventoryStartX(), wearPlaceStart[WearPlace_.MainArmor].Y + InventoryStartY(), game.d_Inventory.MainArmor, 0, 0);
        DrawItem(wearPlaceStart[WearPlace_.Boots].X + InventoryStartX(), wearPlaceStart[WearPlace_.Boots].Y + InventoryStartY(), game.d_Inventory.Boots, 0, 0);
        DrawItem(wearPlaceStart[WearPlace_.Helmet].X + InventoryStartX(), wearPlaceStart[WearPlace_.Helmet].Y + InventoryStartY(), game.d_Inventory.Helmet, 0, 0);
        DrawItem(wearPlaceStart[WearPlace_.Gauntlet].X + InventoryStartX(), wearPlaceStart[WearPlace_.Gauntlet].Y + InventoryStartY(), game.d_Inventory.Gauntlet, 0, 0);
        
        //info
        if (SelectedCell(scaledMouse) != null)
        {
            PointRef selected = SelectedCell(scaledMouse);
            selected.Y += ScrollLine;
            PointRef itemAtCell = inventoryUtil.ItemAtCell(selected);
            if (itemAtCell != null)
            {
                Packet_Item item = GetItem(game.d_Inventory, itemAtCell.X, itemAtCell.Y);
                if (item != null)
                {
                    int x = (selected.X) * CellDrawSize + CellsStartX();
                    int y = (selected.Y) * CellDrawSize + CellsStartY();
                    DrawItemInfo(scaledMouse.X, scaledMouse.Y, item);
                }
            }
        }

        if(SelectedCraftCell(scaledMouse) != null)
        {
            PointRef selected = SelectedCraftCell(scaledMouse);
            PointRef itematcell = inventoryUtil.ItemAtCraftCell(selected);
            if (itematcell != null)
            {
                Packet_Item item = GetCraftItem(game.d_Inventory, itematcell.X, itematcell.Y);
                if (item != null)
                {
                    int x = (selected.X) * CellDrawSize + CellsStartX();
                    int y = (selected.Y) * CellDrawSize + CellsStartY();
                    DrawItemInfo(scaledMouse.X, scaledMouse.Y, item);
                }
            }
        }

        if (SelectedWearPlace(scaledMouse) != null)
        {
            int selected = SelectedWearPlace(scaledMouse).value;
            Packet_Item itemAtWearPlace = inventoryUtil.ItemAtWearPlace(selected, game.ActiveMaterial);
            if (itemAtWearPlace != null)
            {
                DrawItemInfo(scaledMouse.X, scaledMouse.Y, itemAtWearPlace);
            }
        }
        if (SelectedMaterialSelectorSlot(scaledMouse) != null)
        {
            int selected = SelectedMaterialSelectorSlot(scaledMouse).value;
            Packet_Item item = game.d_Inventory.RightHand[selected];
            if (item != null)
            {
                DrawItemInfo(scaledMouse.X, scaledMouse.Y, item);
            }
        }

        if (game.d_Inventory.DragDropItem != null)
        {
            DrawItem(scaledMouse.X, scaledMouse.Y, game.d_Inventory.DragDropItem, 0, 0);
        }
    }

    Packet_Item GetItem(Packet_Inventory inventory, int x, int y)
    {
        for (int i = 0; i < inventory.ItemsCount; i++)
        {
            if (inventory.Items[i].X == x && inventory.Items[i].Y == y)
            {
                return inventory.Items[i].Value_;
            }
        }
        return null;
    }

    Packet_Item GetCraftItem(Packet_Inventory inventory, int x, int y)
    {
        for (int i = 0; i < inventory.ItemsCount; i++)
        {
            if (inventory.CraftItems[i].X == x && inventory.CraftItems[i].Y == y)
            {
                return inventory.CraftItems[i].Value_;
            }
        }
        return null;
    }

    public void DrawMaterialSelector()
    {
        game.Draw2dBitmapFile("materials.png", MaterialSelectorBackgroundStartX(), MaterialSelectorBackgroundStartY(), game.platform.FloatToInt(1024 * game.Scale()), game.platform.FloatToInt(128 * game.Scale()));
        int materialSelectorStartX_ = MaterialSelectorStartX();
        int materialSelectorStartY_ = MaterialSelectorStartY();
        for (int i = 0; i < 10; i++)
        {
            Packet_Item item = game.d_Inventory.RightHand[i];
            if (item != null)
            {
                DrawItem(materialSelectorStartX_ + i * ActiveMaterialCellSize(), materialSelectorStartY_,
                    item, ActiveMaterialCellSize(), ActiveMaterialCellSize());
            }
        }
        game.Draw2dBitmapFile("activematerial2.png",
            MaterialSelectorStartX() + ActiveMaterialCellSize() * game.ActiveMaterial,
            MaterialSelectorStartY(), ActiveMaterialCellSize(), ActiveMaterialCellSize());
    }

    IntRef SelectedWearPlace(PointRef scaledMouse)
    {
        for (int i = 0; i < wearPlaceStartLength; i++)
        {
            //Changes were made, reference was copied instead of  value
            PointRef p = new PointRef();
            p.X = wearPlaceStart[i].X;
            p.Y = wearPlaceStart[i].Y;

            p.X += InventoryStartX();
            p.Y += InventoryStartY();
            PointRef cells = wearPlaceCells[i];
            if (scaledMouse.X >= p.X && scaledMouse.Y >= p.Y
                && scaledMouse.X < p.X + cells.X * CellDrawSize
                && scaledMouse.Y < p.Y + cells.Y * CellDrawSize)
            {
                return IntRef.Create(i);
            }
        }
        return null;
    }

    const int wearPlaceStartLength = 5;
    //indexed by enum WearPlace
    PointRef[] wearPlaceStart;
    //indexed by enum WearPlace
    PointRef[] wearPlaceCells;

    void DrawItem(int screenposX, int screenposY, Packet_Item item, int drawsizeX, int drawsizeY)
    {
        if (item == null)
        {
            return;
        }
        int sizex = dataItems.ItemSizeX(item);
        int sizey = dataItems.ItemSizeY(item);
        if (drawsizeX == 0 || drawsizeX == -1)
        {
            drawsizeX = CellDrawSize * sizex;
            drawsizeY = CellDrawSize * sizey;
        }
        if (item.ItemClass == Packet_ItemClassEnum.Block)
        {
            if (item.BlockId == 0)
            {
                return;
            }
            game.Draw2dTexture(game.terrainTexture, screenposX, screenposY,
                drawsizeX, drawsizeY, IntRef.Create(dataItems.TextureIdForInventory()[item.BlockId]), game.texturesPacked(), Game.ColorFromArgb(255, 255, 255, 255), false);
            if (item.BlockCount > 1)
            {
                FontCi font = new FontCi();
                font.size = 8;
                font.family = "Arial";
                game.Draw2dText(game.platform.IntToString(item.BlockCount), font, screenposX, screenposY, null, false);
            }
        }
        else
        {
            game.Draw2dBitmapFile(dataItems.ItemGraphics(item), screenposX, screenposY,
                drawsizeX, drawsizeY);
        }
    }

    public void DrawItemInfo(int screenposX, int screenposY, Packet_Item item)
    {
        int sizex = dataItems.ItemSizeX(item);
        int sizey = dataItems.ItemSizeY(item);
        IntRef tw = new IntRef();
        IntRef th = new IntRef();
        float one = 1;
        game.platform.TextSize(dataItems.ItemInfo(item), 11 + one / 2, tw, th);
        tw.value += 6;
        th.value += 4;
        int w = game.platform.FloatToInt(tw.value + CellDrawSize * sizex);
        int h = game.platform.FloatToInt(th.value < CellDrawSize * sizey ? CellDrawSize * sizey + 4 : th.value);
        if (screenposX < w + 20) { screenposX = w + 20; }
        if (screenposY < h + 20) { screenposY = h + 20; }
        if (screenposX > game.Width() - (w + 20)) { screenposX = game.Width() - (w + 20); }
        if (screenposY > game.Height() - (h + 20)) { screenposY = game.Height() - (h + 20); }
        game.Draw2dTexture(game.WhiteTexture(), screenposX - w, screenposY - h, w, h, null, 0, Game.ColorFromArgb(255, 0, 0, 0), false);
        game.Draw2dTexture(game.WhiteTexture(), screenposX - w + 2, screenposY - h + 2, w - 4, h - 4, null, 0, Game.ColorFromArgb(255, 105, 105, 105), false);
        FontCi font = new FontCi();
        font.family = "Arial";
        font.size = 10;
        game.Draw2dText(dataItems.ItemInfo(item), font, screenposX - tw.value + 4, screenposY - h + 2, null, false);
        Packet_Item item2 = new Packet_Item();
        item2.BlockId = item.BlockId;
        DrawItem(screenposX - w + 2, screenposY - h + 2, item2, 0, 0);
    }
}
