﻿using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sharepoint.Http.Data.Connector.Models;
using Sharepoint.Http.Data.Connector.Extensions;
using Sharepoint.Http.Data.Connector.Business.Configurations;

namespace Sharepoint.Http.Data.Connector.Business.Commands
{
    /// <summary>
    /// This class contains all the operations that can affect the information hosted in a Sharepoint site.
    /// </summary>
    public class SharepointDataCommands : SharepointDataConfiguration
    {
        public SharepointDataCommands(SharepointContextConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Function to delete a folder from a specific path.
        /// </summary>
        /// <param name="serverRelativeUrl">Relative url of resource.</param>
        /// <returns>Deleted folder from Sharepoint.</returns>
        /// <exception cref="Exception">Sharepoint connection error.</exception>
        public async Task<bool> DeleteResourceAsync(string serverRelativeUrl)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.DELETE_RESOURCE);
                var responseHttp = await client.PostAsync($"_api/web/GetFolderByServerRelativeUrl('{_configuration.ServerRelativeUrl}{serverRelativeUrl}')", null);
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return responseHttp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Function to delete a file from a specific path and file name.
        /// </summary>
        /// <param name="serverRelativeUrl">Relative url of resource.</param>
        /// <param name="fileName">File name to delete.</param>
        /// <returns>Deleted file from Sharepoint.</returns>
        /// <exception cref="Exception">Sharepoint connection error.</exception>
        public async Task<bool> DeleteFileAsync(string serverRelativeUrl, string fileName)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.DELETE_RESOURCE);
                var responseHttp = await client.PostAsync($"_api/web/GetFolderByServerRelativeUrl('{_configuration.ServerRelativeUrl}{serverRelativeUrl}/{fileName}')", null);
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return responseHttp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Function to create a folder to main server relative url.
        /// </summary>
        /// <param name="folderName">Folder name to be created.</param>
        /// <returns>Sharepoint folder information.</returns>
        /// <exception cref="Exception">Sharepoint connection error.</exception>
        public async Task<SharepointFolder?> CreateFolderAsync(string folderName)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.APPJSON_NOMETADATA);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "_api/web/folders");
                request.Content = new StringContent(JsonConvert.SerializeObject(new { ServerRelativeUrl = $"{_configuration.ServerRelativeUrl}{folderName}" }), Encoding.UTF8, "application/json");
                var responseHttp = await client.SendAsync(request);
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return JsonConvert.DeserializeObject<SharepointFolder>(await responseHttp.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Function to create a folder for a specific path.
        /// </summary>
        /// <param name="serverRelativeUrl">Relative url of resource.</param>
        /// <param name="folderName">Folder name to be created.</param>
        /// <returns>Sharepoint folder information.</returns>
        /// <exception cref="Exception">Sharepoint connection error.</exception>
        public async Task<SharepointFolder?> CreateFolderAsync(string serverRelativeUrl, string folderName)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.APPJSON_NOMETADATA);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "_api/web/folders");
                request.Content = new StringContent(JsonConvert.SerializeObject(new { ServerRelativeUrl = $"{_configuration.ServerRelativeUrl}{serverRelativeUrl}/{folderName}" }), Encoding.UTF8, "application/json");
                var responseHttp = await client.SendAsync(request);
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return JsonConvert.DeserializeObject<SharepointFolder>(await responseHttp.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Function to upload a file for a specific path.
        /// </summary>
        /// <param name="serverRelativeUrl">Relative url of resource.</param>
        /// <param name="fileName">File name to delete.</param>
        /// <param name="content">Content file.</param>
        /// <returns>Sharepoint file information.</returns>
        /// <exception cref="Exception">Sharepoint connection error.</exception>
        public async Task<SharepointFile?> UploadFileAsync(string serverRelativeUrl, string fileName, byte[] content)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.APPJSON_NOMETADATA);
                var responseHttp = await client.PostAsync(
                    $"_api/web/GetFolderByServerRelativeUrl('{_configuration.ServerRelativeUrl}{serverRelativeUrl}')/Files/add(overwrite=true, url='{fileName}')",
                    new ByteArrayContent(content)
                );
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return JsonConvert.DeserializeObject<SharepointFile>(await responseHttp.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Fuction to move a resource to recycle bin in a sharepoint site an unique identifier.
        /// </summary>
        /// <param name="serverRelativeUrl">Resource unique identifier.</param>
        /// <returns>Recycle bin resource information.</returns>
        public async Task<Guid> DeleteResourceToRecycleBinByIdAsync(string serverRelativeUrl)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.APPJSON_NOMETADATA);
                var responseHttp = await client.PostAsync(
                    $"_api/web/GetFolderByServerRelativeUrl('{_configuration.ServerRelativeUrl}{serverRelativeUrl}')/recycle", null
                );
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                var response = JObject.Parse(await responseHttp.Content.ReadAsStringAsync());
                return response.Value<string>("value") != string.Empty ? new Guid(response.Value<string>("value")) : Guid.Empty;
            } catch(Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Fuction to restore a resource that is in recycle bin folder using an unique identifier.
        /// </summary>
        /// <param name="resourceId">Resource unique identifier.</param>
        /// <returns>Resource restored.</returns>
        public async Task<bool?> RestoreRecycleBinResourceByIdAsync(Guid resourceId)
        {
            try
            {
                var client = await ConfigureClient(HeaderActionTypes.APPJSON_NOMETADATA);
                var responseHttp = await client.PostAsync($"_api/web/recyclebin('{ resourceId }')/restore", null);
                if (!responseHttp.IsSuccessStatusCode)
                    await responseHttp.ValidateException();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
