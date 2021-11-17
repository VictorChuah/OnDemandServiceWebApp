$(function () {
  // Init
  var $imageContent = $([]);
  $(".button").button();
  $("#card_modal").dialog({
    autoOpen: false,
    dialogClass: "small-title",
    modal: true,
    width: 640,
    height: 540,
    buttons: [{
      text: "Save",
      click: function () {
        var form = $("#drawingForm");
        var image = document.getElementById("preview_image").toDataURL("image/png");
        image = image.replace('data:image/png;base64,', '');

        $(this).dialog("close");
        update(image)
      }
    }, {
      text: "Close",
      click: function () {
        $(this).dialog("close");
      }
    }]
  });

  $(".fig_image").each(function () {
    var figSrc = $(this).data("image-src");
    $(this).css("background-image", "url('" + figSrc + "')");
  }).draggable({
    containment: "#myWidget",
    helper: "clone",
    cursor: "move"
  });

  $("#disp_card").droppable({
    accept: ".fig_image",
    drop: function (e, ui) {
      console.log("Receiving: ", ui.helper);
      if (!ui.helper.hasClass("placed")) {
        addFigure(ui.helper);
      }
    }
  });

  // Utility Functions
  function addDed(txt) {
    var $close = $("<span>", {
      class: "floating top right ui-icon ui-icon-close",
      title: "Remove Image"
    }).click(function (e) {
      removeItem($(e.target).parent());
    });
    var $dedTxt = $("<div>", {
      id: "ded-" + ($(".text").length + 1),
      class: "placed text"
    }).html(txt).append($close).css({
      position: "absolute",
      top: "20px",
      left: "20px",
      "font-size": $("#dedi_font_size option:selected").val() + "pt",
      "font-family": $("#dedi_font option:selected").val(),
      "text-align": $("#dedi_font_align option:selected").val(),
      "min-width": "1em",
      "min-height": "20px",
      "padding-right": "16px"
    });
    makeDrag($dedTxt);
    makeResize($dedTxt);
    $dedTxt.disableSelection();
    $dedTxt.appendTo($("#disp_card")).fadeIn();
  }

  function addFigure($item) {
    var dropPos = $item.position();
    var dropSrc = $item.data("image-src");
    var dropPlace = {
      top: dropPos.top - $("#disp_card").position().top,
      left: dropPos.left - $("#disp_card").position().left
    };
    var $close = $("<span>", {
      class: "floating top right ui-icon ui-icon-close",
      title: "Remove Image"
    }).click(function (e) {
      removeItem($(e.target).parent());
    });
    var $image = $("<div>", {
      id: "fig-" + ($(".placed").length + 1),
      class: "placed image"
    }).data("image-source", dropSrc).css({
      "background-image": "url('" + dropSrc + "')",
      "background-repeat": "no-repeat",
      width: "200px",
      height: "250px",
      position: "absolute",
      top: dropPlace.top + "px",
      left: dropPlace.left + "px"
    }).append($close);
    $item.fadeOut(function () {
      makeDrag($image);
      makeResize($image);
      $image.appendTo($("#disp_card")).fadeIn();
    });
  }

  function makeDrag($item) {
    $item.draggable({
      containment: "#disp_card"
    });
  }

  function makeResize($item) {
    console.log("resize");
    $item.resizable({
      containment: "#disp_card",
      minWidth: 50,
      aspectRatio: !$item.hasClass("text"),
      start: function (e, ui) {
        console.log("113");
        if ($item.hasClass("text")) {
          console.log("114");
          $item.css("border", "1px dashed #ccc");
        }
      },
      resize: function (e, ui) {
        if ($item.hasClass("text")) {
          console.log("120");
          switch (true) {
            case (ui.size.height < 16):
              $item.css("font-size", "11pt");
              break;
            case (ui.size.height < 19):
              $item.css("font-size", "12pt");
              break;
            case (ui.size.height < 24):
              $item.css("font-size", "14pt");
              break;
            case (ui.size.height < 32):
              $item.css("font-size", "18pt");
              break;
            case (ui.size.height < 48):
              $item.css("font-size", "24pt");
              break;
            case (ui.size.height >= 48):
              $item.css("font-size", "36pt");
              break;
          }
        } else {
          console.log("142");
          $item.css("background-size", ui.size.width + "px " + ui.size.height + "px");
        }
      },
      stop: function (e, ui) {
        if ($item.hasClass("text")) {
          $item.css("border", "0");
        }
      }
    });
  }

  function removeItem($item) {
    console.log("Remove Item: ", $item);
    $item.fadeOut(function () {
      $item.remove();
    });
  }

  function createPreview() {
    $imageContent = [];
    var ctx = $("#preview_image")[0].getContext("2d");
    ctx.clearRect(0, 0, 600, 400);
    var $source = $("#disp_card");
    // Find and draw Text items
    var $text = $source.find(".text");
    var $det = {};
    var img;
    $text.each(function (ind, el) {
      $det.type = "Text";
      $det.top = parseInt($(el).css("top").slice(0, -2));
      $det.left = parseInt($(el).css("left").slice(0, -2));
      $det.width = $(el).width();
      $det.height = $(el).height();
      $det.content = $(el).text();
      $det.font = {};
      $det.font.size = $(el).css("font-size");
      $det.font.family = $(el).css("font-family");
      $det.font.align = $(el).css("text-align");
      $imageContent.push($det);
      console.log("Adding to Image: ", $det);
      ctx.font = $det.font.size + " " + $det.font.family;
      ctx.textAlign = $det.font.align;
      ctx.textBaseline = "top";
      ctx.fillText($det.content, $det.left, $det.top, $det.width);
      $det = {};
    });

    // Find and draw Image items
    var $images = $source.find(".image");
    $det = {};
    $images.each(function (ind, el) {
      var $det = {};
      $det.type = "Image";
      $det.top = parseInt($(el).css("top").slice(0, -2));
      $det.left = parseInt($(el).css("left").slice(0, -2));
      $det.width = $(el).width();
      $det.height = $(el).height();
      $det.src = {};
      $det.src.url = $(el).data("image-source");
      $imageContent.push($det);
      img = new Image();
      img.src = $det.src.url;
      $det.src.width = img.width;
      $det.src.height = img.height;
      $det.ratio = $det.width / img.width;
      $(img).on("load", function () {
        console.log("Adding to Image: ", $det);
        ctx.drawImage(img, $det.left, $det.top, $det.src.width * $det.ratio, $det.src.height * $det.ratio);
        $det = {};
      });
    });
    //console.log($imageContent);
  }

  //Button Action Functions
  $("#add_dedi_text").click(function (e) {
    e.preventDefault();
    $("#dedi_form").submit();
  });
  $("#dedi_form").submit(function (e) {
    // Will catch when Enter / Return is hit in form
    e.preventDefault();
    addDed($("#dedi_text").val());
    $("#dedi_text").val("");
  })
  $("#clear_dedi_text").click(function (e) {
    e.preventDefault();
    $("#dedi_text").val("");
  });
  $("#save_display").click(function (e) {
    e.preventDefault();
    createPreview();
    $("#card_modal").dialog("open");
  });

  $("#clear_display").click(function (e) {
    e.preventDefault();
    if (confirm("Are you sure you wish to clear everything?")) {
      $("#disp_card").html("");
    }
  });
});

$('#file').change(e => {
  let f = e.target.files[0];

  if (f && f.type.startsWith('image/')) {
    crop(f, 300, 300, 'dataURL', 'image/webp').then(url => {
      var gene = $("<li><div class='fig_image' data-image-src=" + url + "></div></li>")
      $('#fig_list').append(gene);
      console.log(url)
    });
  }
  e.target.value = null;

  setTimeout(() => {
    $(".fig_image").each(function () {
      var figSrc = $(this).data("image-src");
      $(this).css("background-image", "url('" + figSrc + "')");
    }).draggable({
      containment: "#myWidget",
      helper: "clone",
      cursor: "move"
    });
  }, 1000)

});


function crop(f, w, h, to = 'blob', type = 'image/jpeg') {
  return new Promise((resolve, reject) => {
    const img = document.createElement('img');

    img.onload = e => {
      URL.revokeObjectURL(img.src);

      // Resize algorithm ---------------------------
      let ratio = w / h;

      let nw = img.naturalWidth;
      let nh = img.naturalHeight;
      let nratio = nw / nh;

      let sx, sy, sw, sh;

      if (ratio >= nratio) {
        // Retain width, calculate height
        sw = nw;
        sh = nw / ratio;
        sx = 0;
        sy = (nh - sh) / 2;
      }
      else {
        // Retain height, calculate width
        sw = nh * ratio;
        sh = nh;
        sx = (nw - sw) / 2;
        sy = 0;
      }
      // --------------------------------------------

      const can = document.createElement('canvas');
      can.width = w;
      can.height = h;
      can.getContext('2d').drawImage(img, sx, sy, sw, sh, 0, 0, w, h);

      // Resolve to blob or dataURL
      if (to == 'blob') {
        can.toBlob(blob => resolve(blob), type);
      }
      else if (to == 'dataURL') {
        let dataURL = can.toDataURL(type);
        resolve(dataURL);
      }
      else {
        reject('ERROR: Specify blob or dataURL');
      }
    };

    img.onerror = e => {
      URL.revokeObjectURL(img.src);
      reject('ERROR: File is not an image');
    };

    img.src = URL.createObjectURL(f);
  });
}

// PURPOSE: Best-fit image within the width and height specified (no upscale)
function fit(f, w, h, to = 'blob', type = 'image/jpeg') {
  return new Promise((resolve, reject) => {
    const img = document.createElement('img');

    img.onload = e => {
      URL.revokeObjectURL(img.src);

      // Resize algorithm ---------------------------
      let ratio = w / h;

      let nw = img.naturalWidth;
      let nh = img.naturalHeight;
      let nratio = nw / nh;

      if (nw <= w && nh <= h) {
        // Smaller than targetted width and height, do nothing
        w = nw;
        h = nh;
      }
      else {
        if (nratio >= ratio) {
          // Retain width, calculate height
          h = w / nratio;
        }
        else {
          // Retain height, calculate width
          w = h * nratio;
        }
      }
      // --------------------------------------------

      const can = document.createElement('canvas');
      can.width = w;
      can.height = h;
      can.getContext('2d').drawImage(img, 0, 0, w, h);

      // Resolve
      if (to == 'blob') {
        can.toBlob(blob => resolve(blob), type);
      }
      else if (to == 'dataURL') {
        let dataURL = can.toDataURL(type);
        resolve(dataURL);
      }
      else {
        reject('ERROR: Specify blob or dataURL');
      }
    };

    img.onerror = e => {
      URL.revokeObjectURL(img.src);
      reject('ERROR: File is not an image');
    };

    img.src = URL.createObjectURL(f);
  });
}

//update name card 
function update(card) {
  console.log("1234")
  console.log($("#imageData").val())

  $.post('', { img: card })

    //// empty post('') means post to the default controller, 
    ///we are not pacifying different action or controller
    /// however we can define a url as following:
    /// var url = "@(Url.Action("GetDataAction", "GetDataController"))"

    .error(function () { alert('Error') })
    .success(function () { alert('OK') })
  return false;
}
