let i = 0;

function AddTag() {
    //Get reference to to input element.
    var tagEntry = document.getElementById("TagEntry");

    //Check if entry is an empty or duplicate tag.
    let result = search(tagEntry.value);

    if (result != null) {
        //Display error message.
        /*Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: result,
            footer: '<a href="">Why do I have this issue?</a>'
        })*/
        swalDarkBtn.fire({
            html: `<span class='font-weight-bolder'>${result}</span>`
        });
    }
    else {

        //Create new Select option.
        let newOption = new Option(tagEntry.value, tagEntry.value);
        document.getElementById("TagList").options[i++] = newOption;
    }

    //Clear out the TagEntry control.
    tagEntry.value = "";
    return true;
}

function DeleteTag() {
    let tagCount = 1;
    let tagList = document.getElementById("TagList");

    if (!tagList) {
        return false;
    }
    if (tagList.selectedIndex == -1) {
        swalDarkBtn.fire({
            html: "<span class='font-weight-bolder'>Choose a tag to delete</span>"
        });
        return true;
    }

    while (tagCount > 0) {

        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount;
        }
        else {
            tagCount = 0;
        }
        i--;
    }
}

$("form").on("submit", function () {
    $("#TagList option").prop("selected", "selected");
})

//Look for the tagValues variable to see if it has data.
if (tagValues != '') {
    let tagArray = tagValues.split(",");
    for (let j = 0; j < tagArray.length; j++) {
        //Load up or replace the options that we have.
        ReplaceTag(tagArray[j], j);
        i++;
    }
}

function ReplaceTag(tag, i) {
    let newOption = new Option(tag, tag);
    document.getElementById("TagList").options[i] = newOption;

}

//Detect empty/duplicate tags and produce error message.
function search(str) {
    if (str == "") {
        return 'Empty tags are not permitted'
    }

    var tagsElement = document.getElementById('TagList');
    if (tagsElement) {
        let options = tagsElement.options;
        for (let i = 0; i < options.length; i++) {
            if (options[i].value == str) {
                return `The tag '#${str}' is not permitted as it is a duplicate tag`;
            }
        }
    }
}

const swalDarkBtn = Swal.mixin({
    customClass: {
        confirmBtn: 'btn btn-danger btn-sm btn-block btn-outline-dark',
    },
    imageUrl: '/assets/img/error.png',
    timer: 5000,
    buttonStyling: false
});
/*
 if (tagEntry != null) {
    //Create new Select option.
    let newOption = new Option(tagEntry);
    document.getElementById("TagList").options[i++] = newOption;

    //Clear out the TagEntry control.
        
    return true;
    }*/