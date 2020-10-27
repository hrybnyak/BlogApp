export const BaseUrl = "https://localhost:44337/api"

export const ApiPaths = {
    Posts: '/articles',
    Blogs: '/blogs',
    Comments: '/comments',
    Categories: '/tegs',
    Users: '/accounts',
    Auth: '/auth',
    TextFilter: '?text=',
    TegFilter: '?tegs='
}

let applicationPaths = {
    Base: '',
    Home: `home`,
    Blogs: `my-blogs`,
    Blog: `my-blogs/:id`,
    Profile: `user-profile`,
    AuthMode: `auth/:mode`,
    Login: `auth/login`,
    Register: `auth/register`,
    Posts: 'posts',
    Post: `my-blogs/:blogId/posts/:id`,
    PostView: 'posts/view',
    BlogView: 'my-blogs/view/',
    PostViewId: 'posts/view/:id',
    BlogViewId: 'my-blogs/view/:id'
}

export const ApplicationPaths = applicationPaths;