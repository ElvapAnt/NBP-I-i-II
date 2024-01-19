import React, { useState } from 'react';
import './Searchbar.css'
import { Button } from '@mui/material';

const SearchBar = ({query,setQuery,onQueryExecute}) => {



    function search(e){
        e.preventDefault()
        setQuery(e.target.value)
    }

    return (
        <div className="searchbar_container">
            <input
                type="text"
                placeholder="Search"
                onChange={search}
                value={query}
            />
            <Button onClick={ev=>onQueryExecute(query)}>🔍</Button>
        </div>
    );
};

export default SearchBar;