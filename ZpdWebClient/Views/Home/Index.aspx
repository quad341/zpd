<%@ Page Title="Title" Language="C#" Inherits="System.Web.Mvc.ViewPage<ZpdWebClient.ZPDService.ZpdCurrentPlayerState>" MasterPageFile="~/Views/Shared/Site.master" %>

<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Welcome to ZPD. We are the next Jukebox</asp:Content>

<asp:Content runat="server" ID="Head" ContentPlaceHolderID="HeadContent">
        <script type="text/javascript">
            var currentState = { 'Name': "", 'Artist': "", 'CurrentTrackPosition': 0 };

            setInterval(UpdateCurrentStatus, 1000);
            function UpdateCurrentStatus() {
                $.get('home/GetCurrentPlayerState', null, function (data) {
                    $('#currentTrackName').html(data.Name);
                    $('#currentTrackArtists').html(data.Artist);
                    $('#currentTrackPosition').html(data.CurrentTrackPosition);
                });
            }
    </script>
</asp:Content>


<asp:Content ID="BodyContent" runat="server" contentplaceholderid="MainContent">
    Index page. We'll want a search box in the middle and the status on the right
    
    <div>
        We're currently playing <span id="currentTrackName"><%= Model.CurrentTrack.Name %></span> by <span id="currentTrackArtists"><%= Model.CurrentTrack.Artist %></span>. It is <span id="currentTrackPosition"><%= Model.CurrentTrackPosition %></span> seconds in.
    </div>
</asp:Content>




