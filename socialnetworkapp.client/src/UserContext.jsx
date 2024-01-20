import { useState } from "react";
import { CURRENT_USER } from "./Constants";
import React from "react";

let localUser= localStorage.getItem(CURRENT_USER)




export const UserContext = React.createContext({})

export const UserContextProvider = ({ children }) => {

    const [currentUser, setCurrentUser1] = useState(
        (localUser=='' || localUser==null)?null:JSON.parse(localUser)
    )
    function setCurrentUser(user)
    {   
    localStorage.setItem(CURRENT_USER,JSON.stringify(user))
    setCurrentUser1(user)
    }   
    return <UserContext.Provider value={{ currentUser, setCurrentUser }}>
        {children}
    </UserContext.Provider>
}