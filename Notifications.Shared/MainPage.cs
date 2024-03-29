﻿

using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Notifications
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    partial class MainPage
    {
        private App viewModel;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.StatusMessage = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    this.viewModel.StatusMessage = "This sample app requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    // Create a Tile.
                    // WARNING! This tile guid is only an example. 
                    // always create a unique guid for each application.
                    // If one application installs its tile, a second application using the same guid will fail to install
                    // its tile due to a guid conflict. In the event of such a failure, the text of the exception will not
                    // report that the tile with the same guid already exists on the band.
                    // There might be other unexpected behavior.
                    Guid myTileId = new Guid("06CF1D20-81F5-4200-AA8B-C694BAFDA0A8");
                    BandTile myTile = new BandTile(myTileId)
                    {
                        Name = "My Tile",
                        TileIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconLarge.png"),
                        SmallIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconSmall.png")
                    };

                    // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
                    // But in case you modify this sample code and run it again, let's make sure to start fresh.
                    await bandClient.TileManager.RemoveTileAsync(myTileId);
                    
                    // Check if there is space for your tile on the band before adding it.
                    if (await bandClient.TileManager.GetRemainingTileCapacityAsync() > 0)
                    {
                        // Create the Tile on the Band.
                        await bandClient.TileManager.AddTileAsync(myTile);

                        // Send a notification.
                        await bandClient.NotificationManager.SendMessageAsync(myTileId, "contextere", "You have new Job Notification at 99 Viewmount Drive ", DateTimeOffset.Now, MessageFlags.ShowDialog);

                        this.viewModel.StatusMessage = "Done. Check the Tile on your Band (it's the last Tile).";
                    }
                    else
                    {
                        this.viewModel.StatusMessage = "No space on the band for your tile, remove a tile and try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                this.viewModel.StatusMessage = ex.ToString();
            }
        }

        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }
    }
}
