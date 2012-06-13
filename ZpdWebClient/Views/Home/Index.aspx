<%@ Page Title="Title" Language="C#" Inherits="System.Web.Mvc.ViewPage<ZpdWebClient.ZPDService.ZpdCurrentPlayerState>" MasterPageFile="~/Views/Shared/Site.master" %>

<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Welcome to ZPD. We are the next Jukebox</asp:Content>

<asp:Content runat="server" ID="Head" ContentPlaceHolderID="HeadContent">
        <style type="text/css">
            .evenrow { background-color: #E5E5E5; }
        </style>
        <script type="text/javascript">
            function PerformSearch() {
                var query = $("#search").val();
                $.get('home/Search?query=' + query, null, function (data) {
                    var results = $("#results");
                    results.html('<tr><th>Trace Name</th><th>Artist</th><th>Album</th><th>Enqueue</th></tr>');
                    for (var i = 0; i < data.length; i++) {
                        results.append("<tr id='" + data[i].MediaId + '-' + data[i].MediaTypeId + "'><td>" + data[i].Name + "</td><td>" + data[i].Artist + "</td><td>" + data[i].Album + "</td><td><button type='button'>Enqueue</button></td></tr>");
                    }
                    $("#results tr:even").addClass("evenrow");

                    $('#results button').click(function() {
                        var source = $(this).parents('tr')[0];
                        if ("results" != source.id) {
                            var parts = source.id.split('-');
                            $.post('home/QueueTrack', { MediaId: parts[0], MediaTypeId: parts[1] }, function() {
                                $(source).fadeOut('slow');
                                UpdateSongQueue();
                            });
                        }
                    });
                });
                
                
            }
        </script>
</asp:Content>


<asp:Content ID="BodyContent" runat="server" contentplaceholderid="MainContent">
    
    <div>
        <h3>Search</h3>
        <form action="#" onsubmit="PerformSearch(); return false;">
            <input type="text" id="search" name="search" size="200"/>
            <button>Search</button>
        </form>
        <table id="results"><tr id="header"><th>Trace Name</th><th>Artist</th><th>Album</th><th>Enqueue</th></tr></table>
    </div>
</asp:Content>




