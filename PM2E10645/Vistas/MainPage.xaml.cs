using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PM2E10645.Models;
using Xamarin.Essentials;
using System.Threading;
using Plugin.Media;
using System.IO;



namespace PM2E10645.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        //----- Constructor-----
        byte[] imgtoSave;
        public MainPage()
        {
            InitializeComponent();

            LongitudLatitud();

            Descripcion_input.Text = "";
            imgphotoubicacion.Source = null;
        }

        /*Se encarga de obtener la longitud y latitud de la ubicación del dispositivo
          a través de la geolocalización*/
        public async void LongitudLatitud()
        {
            try
            {
                var georequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

                var tokendecancelacion = new CancellationTokenSource();

                var localizacion = await Geolocation.GetLocationAsync(georequest, tokendecancelacion.Token);


                if (localizacion != null)
                {
                    Latitud_input.Text = localizacion.Latitude.ToString();
                    Longitud_input.Text = localizacion.Longitude.ToString();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Alerta", "Este dispositivo no soporta GPS", "Ok");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Error", "Error de Dispositivo", "Ok");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Alerta", "Sin Permisos de Geolocalizacion", "Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alerta", "Sin Ubicacion", "Ok");
            }
        }


        //-----Campos Vacios------
        private async Task validarFormulario()
        {

            if (String.IsNullOrWhiteSpace(Longitud_input.Text) || String.IsNullOrWhiteSpace(Latitud_input.Text) || String.IsNullOrWhiteSpace(Descripcion_input.Text) || imgphotoubicacion.Source == null)
            {
                await this.DisplayAlert("Obligatorio", "¡Debe Llenar los campos vacios!", "OK");
            }

        }


        //--Tomar la foto---
        private async void btntomarphoto_Clicked(object sender, EventArgs e)
        {
            var tomarphoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "PhotoLocationApp",
                Name = DateTime.Now.ToString() + "_Photo.jpg",
                SaveToAlbum = true
            });

            if (tomarphoto != null)
            {
                imgtoSave = null;
                MemoryStream memoryStream = new MemoryStream();

                tomarphoto.GetStream().CopyTo(memoryStream);
                imgtoSave = memoryStream.ToArray();

                imgphotoubicacion.Source = ImageSource.FromStream(() => { return tomarphoto.GetStream(); });
            }
        }

        //---GetImgBytes se utiliza para convertir un flujo de datos---

        private byte[] GetImgBytes(Stream stream)
        {
            byte[] ImgBytes;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                stream.CopyTo(memoryStream);
                ImgBytes = memoryStream.ToArray();
            }
            return ImgBytes;
        }

        //---Accion de boton guardar---
        private async void btnaguardar_Clicked(object sender, EventArgs e)
        {
            if (validarFormulario().IsCompleted)
            {
                try
                {
                    var ubicacion = new Ubicaciones
                    {
                        Imagen = imgtoSave,
                        Longitud = (float)Convert.ToDouble(Longitud_input.Text),
                        Latitud = (float)Convert.ToDouble(Latitud_input.Text),
                        Descripcion = Descripcion_input.Text
                    };

                    var resultado = await App.BaseDatos.GuardarUbicacion(ubicacion);

                    if (resultado != 0)
                    {
                        await DisplayAlert("Guardado", "¡Se guardo correctamente!", "Ok");
                        await Navigation.PushAsync(new ListViewLocationsP());
                        cleanScreen();
                    }
                    else
                    {
                        await DisplayAlert("Alerta", "Ha Ocurrido un Error", "Ok");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message.ToString(), "Ok");
                }
            }

        }

        //---Lleva a las ventana de vitios visitados---
        private async void btnlistviewubicaciones_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListViewLocationsP());
        }

        //---Para salir de la aplicacion---
        private void btnsalir_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        //---Limpia la pantalla para que hagamos un nuevo registro---
        private async void cleanScreen()
        {
            LongitudLatitud();
            this.Descripcion_input.Text = String.Empty;
            imgphotoubicacion.Source = null;
        }
    }
}