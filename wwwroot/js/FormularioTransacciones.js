function inicializarFormularioTransacciones(urlObtenerCategorias) {

    //este campo es el id de la cascada
    $("#TipoOperacionId").change(async function () {
        const valorseleccionado = $(this).val();
        //enviamso la url, y usamos fetch, que es la que usa javascript para consumir apis o url
        //almacenamos el valor de la repsuesta del webapi
        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            body: valorseleccionado,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await respuesta.json();
        //console.log(json);
        //una vez verificada que la informacion llea de forma correcta, realizaremso el mapeo
        //de todo el json que nos devuelve para pdoer asi cambiar en la vista en el desplegable
        //transofrmandolas en opciones, osea un arreglo de opciones
        const opciones =
            json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        //insertamos nuestro arreglo de opciones en el html
        $("#CategoriaId").html(opciones);
    })

    
}