import { useState } from "react"
import { CURRENT_USER,CURRENT_CHAT } from "../../Constants"
import SearchBar from "../Searchbar/Searchbar"
import { Button } from "@mui/material"
import './Navbar.css'
import { Outlet, useNavigate } from "react-router-dom"


    
export function Navbar({ children }) {
    const navigate = useNavigate()

    async function searchForUser(query)
    {
        try {
          navigate('search/'+query)
        }
        catch (error)
        {
            console.log(error.message)
        }
 
}
    const [query, setQuery] = useState("")
    let currentUser = localStorage.getItem(CURRENT_USER)
    const enabled = currentUser != '' && currentUser != null
    if (enabled)
    {
        currentUser=JSON.parse(currentUser)
    }
    else
    {
        }
    
    return <div id="root">
        {enabled && <nav className="navbar">
            <SearchBar query={query} setQuery={setQuery} onQueryExecute={(ev) => searchForUser(query)} />
            <ul>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            localStorage.removeItem(CURRENT_USER)
                            sessionStorage.removeItem(CURRENT_CHAT)
                            navigate('/login')
                        }
                    }>
                        Log out
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            navigate('/notifications')
                        }
                    }>
                        Notifications
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            navigate('/chat')
                        }
                    }>
                        Messages
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            navigate('/profile/'+currentUser.userId)
                        }
                    }>
                        Profile
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                           navigate('/add_post')
                        }
                    }>
                        Add post
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            navigate('/friends')
                        }
                    }>
                        Friends
                    </Button>
                </li>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            navigate('/home')
                        }
                    }>
                        Home
                    </Button>
                </li>
            </ul>
        </nav>}
        <Outlet/>
        </div>
}