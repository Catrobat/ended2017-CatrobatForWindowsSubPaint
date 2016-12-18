namespace Catrobat.Paint.WindowsPhone.Command
{
    class RemoveCommand : CommandBase
    {

        public RemoveCommand()
        {
        }


        public override bool ReDo()
        {
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
            return true;
        }

        public override bool UnDo()
        {
            PocketPaintApplication.GetInstance().PaintingAreaView.AlignPositionOfGridWorkingSpace(null);
            return true;
        }
    }
}
