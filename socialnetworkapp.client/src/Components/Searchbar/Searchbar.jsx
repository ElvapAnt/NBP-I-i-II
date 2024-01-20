import React, { useState } from 'react';
import './Searchbar.css'
import { Button } from '@mui/material';

const SearchBar = ({query,setQuery,onQueryExecute,customization}) => {



    function search(e){
        e.preventDefault()
        setQuery(e.target.value)
    }
    let buttonImg = undefined
    let placeholder = undefined
    if (customization)
    {
        buttonImg = customization.buttonImg
        placeholder=customization.placeholder
        }
    return (
        <div className="searchbar_container">
            <input
                type="text"
                placeholder={placeholder == undefined ? "Search" : placeholder}
                onChange={search}
                value={query}
            />
            <Button onClick={ev=>onQueryExecute(query)}>{buttonImg==undefined?'🔍':buttonImg}</Button>
        </div>
    );
};

export default SearchBar;