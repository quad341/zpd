<%@ Page Title="Title" Language="C#" Inherits="System.Web.Mvc.ViewPage<ZpdWebClient.ZPDService.ZpdCurrentPlayerState>" MasterPageFile="~/Views/Shared/Site.master" %>

<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Welcome to ZPD. We are the next Jukebox</asp:Content>


<asp:Content ID="BodyContent" runat="server" contentplaceholderid="MainContent">
    Index page. We'll want a search box in the middle and the status on the right
    
    <div>
        We're currently playing <%= Model.CurrentTrack.Name %> by <%= Model.CurrentTrack.Artist %>. It is <%= Model.CurrentTrackPosition %> seconds in.
    </div>
</asp:Content>




