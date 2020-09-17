using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Vision;
using static Android.Gms.Vision.MultiProcessor;
using Android.Gms.Vision.Faces;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;

using System;
using System.Threading.Tasks;

using CruzeBio.Shared;

namespace CruzeBio
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IFactory
    {
        private static readonly string TAG = "FaceTracker";
        private static readonly int RC_HANDLE_GMS = 9001;
        
        // permission request codes need to be < 256
        private static readonly int RC_HANDLE_CAMERA_PERM = 2;

        private CameraSource mCameraSource = null;
        private CameraSourcePreview mPreview;
        private GraphicOverlay mGraphicOverlay;

        public static string GreetingsText
        {
            get;
            set;
        }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            mPreview = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            mGraphicOverlay = FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
            {
                CreateCameraSource();
                CruzeBioHelper.Init();
                CruzeBioHelper.GreetingsCallback = (s) => { RunOnUiThread(()=> GreetingsText = s ); };
                await CruzeBioHelper.RegisterFaces();
            }
            else { RequestCameraPermission(); }

        }

        protected override void OnResume()
        {
            base.OnResume();
            StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mPreview.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mCameraSource != null)
            {
                mCameraSource.Release();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RequestCameraPermission()
        {
            Log.Warn(TAG, "Camera permission is not granted. Requesting permission");

            var permissions = new string[] { Manifest.Permission.Camera };

            if (!ActivityCompat.ShouldShowRequestPermissionRationale(this,
                Manifest.Permission.Camera))
            {
                ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM);
                return;
            }

            Snackbar.Make(mGraphicOverlay, Resource.String.permission_camera_rationale,
                Snackbar.LengthIndefinite)
                .SetAction(Resource.String.ok, (o) => { ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM); })
                .Show();
        }

        /**
        * Creates and starts the camera.  Note that this uses a higher resolution in comparison
        * to other detection examples to enable the barcode detector to detect small barcodes
        * at long distances.
        */
        private void CreateCameraSource()
        {

            var context = Application.Context;
            FaceDetector detector = new FaceDetector.Builder(context)
                .SetClassificationType(ClassificationType.All)
                .Build();

            detector.SetProcessor(
                new MultiProcessor.Builder(this)
                    .Build());

            if (!detector.IsOperational)
            {
                // Note: The first time that an app using face API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any faces.
                //
                // isOperational() can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
                Log.Warn(TAG, "Face detector dependencies are not yet available.");
            }

            mCameraSource = new CameraSource.Builder(context, detector)
                    .SetRequestedPreviewSize(640, 480)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedFps(30.0f)
                    .Build();


        }

        /**
        * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
        * (e.g., because onResume was called before the camera source was created), this will be called
        * again when the camera source is created.
         */
        private void StartCameraSource()
        {

            // check that the device has play services available.
            int code = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.ApplicationContext);
            if (code != ConnectionResult.Success)
            {
                Dialog dlg = GoogleApiAvailability.Instance.GetErrorDialog(this, code, RC_HANDLE_GMS);
                dlg.Show();
            }

            if (mCameraSource != null)
            {
                try
                {
                    mPreview.Start(mCameraSource, mGraphicOverlay);
                }
                catch (System.Exception e)
                {
                    Log.Error(TAG, "Unable to start camera source.", e);
                    mCameraSource.Release();
                    mCameraSource = null;
                }
            }
        }

        public Tracker Create(Java.Lang.Object item)
        {
            return new GraphicFaceTracker(mGraphicOverlay, mCameraSource);
        }

    }


    class GraphicFaceTracker : Tracker, CameraSource.IPictureCallback
    {
        private GraphicOverlay mOverlay;
        private FaceGraphic mFaceGraphic;
        private CameraSource mCameraSource = null;
        private bool isProcessing = false;

        public GraphicFaceTracker(GraphicOverlay overlay, CameraSource cameraSource = null)
        {
            mOverlay = overlay;
            mFaceGraphic = new FaceGraphic(overlay);
            mCameraSource = cameraSource;
        }

        public override void OnNewItem(int id, Java.Lang.Object item)
        {
            mFaceGraphic.SetId(id);
            if (mCameraSource != null && !isProcessing)
                mCameraSource.TakePicture(null, this);
        }

        public override void OnUpdate(Detector.Detections detections, Java.Lang.Object item)
        {
            var face = item as Face;
            mOverlay.Add(mFaceGraphic);
            mFaceGraphic.UpdateFace(face);

        }

        public override void OnMissing(Detector.Detections detections)
        {
            mOverlay.Remove(mFaceGraphic);

        }

        public override void OnDone()
        {
            mOverlay.Remove(mFaceGraphic);

        }

        public void OnPictureTaken(byte[] data)
        {
            Task.Run(async () =>
            {
                try
                {
                    isProcessing = true;

                    Console.WriteLine("face detected: ");
                    /*
                    var imageAnalyzer = new ImageAnalyzer(data);
                    await CruzeBioHelper.ProcessCameraCapture(imageAnalyzer);
                    */
                }

                finally
                {
                    isProcessing = false;


                }

            });
        }
    }
}
