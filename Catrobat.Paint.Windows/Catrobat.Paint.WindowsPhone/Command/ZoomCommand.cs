using Windows.UI.Xaml.Media;

namespace Catrobat.Paint.WindowsPhone.Command
{
    class ZoomCommand : CommandBase
    {
        private TransformGroup _transformGroup;

        public ZoomCommand(TransformGroup transformGroup)
        {
            _transformGroup = transformGroup;
        }

        public override bool ReDo()
        {
            TransformGroup transformGroupWithAllKindOfTransforms = ((TransformGroup)PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform);
            if (transformGroupWithAllKindOfTransforms != null)
            {
                if (_transformGroup.Children.Count == 0)
                {
                    transformGroupWithAllKindOfTransforms.Children.Clear();
                    PocketPaintApplication.GetInstance().PaintingAreaView.AlignPositionOfGridWorkingSpace(null);
                    PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnableOfAppBarButtonResetZoom(false);
                }
                else
                {
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        transformGroupWithAllKindOfTransforms.Children.Add(_transformGroup.Children[i]);
                    }
                    PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnableOfAppBarButtonResetZoom(true);
                }
            }
            PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform = transformGroupWithAllKindOfTransforms;
            PocketPaintApplication.GetInstance().GridWorkingSpace.UpdateLayout();
            PocketPaintApplication.GetInstance().GridWorkingSpace.InvalidateArrange();
            PocketPaintApplication.GetInstance().GridWorkingSpace.InvalidateMeasure();

            return true;
        }

        public override bool UnDo()
        {
            if (CommandManager.GetInstance().doesCommandTypeExistInUndoList(typeof(ZoomCommand)))
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnableOfAppBarButtonResetZoom(true);
            }
            else
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnableOfAppBarButtonResetZoom(false);
            }
            TransformGroup transformGroup = ((TransformGroup)PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform);

            if (transformGroup != null)
            {
                transformGroup.Children.Clear();

                PocketPaintApplication.GetInstance().PaintingAreaView.AlignPositionOfGridWorkingSpace(null);

                PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform = transformGroup;
            }
            PocketPaintApplication.GetInstance().GridWorkingSpace.UpdateLayout();
            PocketPaintApplication.GetInstance().GridWorkingSpace.InvalidateArrange();
            PocketPaintApplication.GetInstance().GridWorkingSpace.InvalidateMeasure();

            return true;
        }
    }
}
