using Microsoft.Maui.Controls.Handlers.Compatibility;
using UIKit;

namespace TrackTop.Platforms.iOS;

public class TrackTopTabbedRenderer : TabbedRenderer
{
    private bool _imagesReapplied;

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        ReapplyTabItemImages();
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        ReapplyTabItemImages();
    }

    private void ReapplyTabItemImages()
    {
        if (_imagesReapplied || ViewControllers is null)
            return;

        bool reapplied = false;
        foreach (var viewController in ViewControllers)
        {
            if (viewController.TabBarItem?.Image is null)
                continue;

            var image = viewController.TabBarItem.Image;
            viewController.TabBarItem.Image = null;
            viewController.TabBarItem.Image = image;
            reapplied = true;
        }

        if (reapplied)
        {
            TabBar.SetNeedsLayout();
            TabBar.LayoutIfNeeded();
            _imagesReapplied = true;
        }
    }
}